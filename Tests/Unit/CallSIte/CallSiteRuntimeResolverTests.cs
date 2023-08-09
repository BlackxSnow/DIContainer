using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DIContainer;
using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;
using DIContainer.Provider;
using DIContainer.Service;
using Xunit;
using Xunit.Abstractions;
using IServiceProvider = DIContainer.Provider.IServiceProvider;

namespace Tests.Unit.CallSIte
{
    public class CallSiteRuntimeResolverTests
    {
        private readonly ITestOutputHelper _TestOutputHelper;

        public CallSiteRuntimeResolverTests(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
        }

        public class InjectorMock : IInjectorRuntimeResolver
        {
            public void Inject(InjectorCallSite callSite, InjectorRuntimeResolverContext context)
            {
                
            }
        }

        private class IntService
        {
            public int Value { get; set; }
        }

        private class StringService
        {
            public string Value { get; set; }
        }

        private class Service
        {
            public IntService IntDep { get; }
            public StringService StringDep { get; }

            public Service(IntService iService, StringService sService)
            {
                IntDep = iService;
                StringDep = sService;
            }
        }
        
        private ConstructorCallSite BuildConstructorCallSite(Type serviceType, ServiceCallSite[] paramCallSites = null)
        {
            var cacheInfo = new ServiceCacheInfo(ServiceLifetime.Transient, new ServiceIdentifier(serviceType));
            ConstructorInfo constructor = serviceType.GetConstructors()[0];
            return new ConstructorCallSite(cacheInfo, null, serviceType, constructor,
                paramCallSites ?? Array.Empty<ServiceCallSite>());
        }

        [Fact]
        public void ConstantCallSite()
        {
            var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());
            var callSite = new ConstantCallSite(typeof(Service), new Service(new IntService(), new StringService()));
            object serviceObject = resolver.Resolve(callSite, new ServiceProviderScope(null, true));
            var service = (Service)serviceObject;
            Assert.IsType<Service>(serviceObject);
            Assert.NotNull(service);
            Assert.NotNull(service.IntDep);
            Assert.NotNull(service.StringDep);
        }
        
        [Fact]
        public void PrimaryConstructorCallSite()
        {
            var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());

            var intCallSite = new ConstantCallSite(typeof(IntService), new IntService());
            var stringCallSite = new ConstantCallSite(typeof(StringService), new StringService());

            ConstructorCallSite serviceCallSite =
                BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });

            object serviceObject = resolver.Resolve(serviceCallSite, new ServiceProviderScope(null, true));
            var service = (Service)serviceObject;
            Assert.IsType<Service>(serviceObject);
            Assert.NotNull(service);
            Assert.NotNull(service.IntDep);
            Assert.NotNull(service.StringDep);
        }

        [Fact]
        public void SecondaryConstructorCallSite()
        {
            var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());

            var intCallSite = BuildConstructorCallSite(typeof(IntService));
            var stringCallSite = BuildConstructorCallSite(typeof(StringService));

            ConstructorCallSite serviceCallSite =
                BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });

            object serviceObject = resolver.Resolve(serviceCallSite, new ServiceProviderScope(null, true));
            Assert.IsType<Service>(serviceObject);
            var service = (Service)serviceObject;
            Assert.NotNull(service);
            Assert.NotNull(service.IntDep);
            Assert.NotNull(service.StringDep);
        }
        
        [Fact]
        public void PrimaryEnumerableCallSite()
        {
            var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());

            var intCallSite = new ConstantCallSite(typeof(IntService), new IntService());
            var stringCallSite = new ConstantCallSite(typeof(StringService), new StringService());

            ConstructorCallSite singleServiceCallSite =
                BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });

            var enumerableCacheInfo = new ServiceCacheInfo(ServiceLifetime.Transient,
                new ServiceIdentifier(typeof(IEnumerable<Service>)));
            
            var enumerableCallSite = new EnumerableCallSite(enumerableCacheInfo, typeof(Service),
                new ServiceCallSite[] { singleServiceCallSite, singleServiceCallSite });

            object serviceObjects = resolver.Resolve(enumerableCallSite, new ServiceProviderScope(null, true));
            
            Assert.IsAssignableFrom<IEnumerable<Service>>(serviceObjects);
            Service[] services = ((IEnumerable<Service>)serviceObjects).ToArray();
            Assert.Equal(2, services.Length);
            Assert.All(services, Assert.NotNull);
            Assert.All(services, s => Assert.NotNull(s.IntDep));
            Assert.All(services, s => Assert.NotNull(s.StringDep));
            Assert.Distinct(services);
        }

        private class EnumerableService
        {
            public IEnumerable<IntService> IntServices;

            public EnumerableService(IEnumerable<IntService> intServices)
            {
                IntServices = intServices;
            }
        }
        
        [Fact]
        public void SecondaryEnumerableCallSite()
        {
            var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());

            ConstructorCallSite intCallSite = BuildConstructorCallSite(typeof(IntService));
            var enumerableIntCacheInfo = new ServiceCacheInfo(ServiceLifetime.Transient,
                new ServiceIdentifier(typeof(IEnumerable<IntService>)));
            var intEnumerableCallSite = new EnumerableCallSite(enumerableIntCacheInfo, typeof(IntService),
                new ServiceCallSite[] { intCallSite, intCallSite });
            
            ConstructorCallSite singleServiceCallSite =
                BuildConstructorCallSite(typeof(EnumerableService), new ServiceCallSite[] { intEnumerableCallSite });

            object serviceObject = resolver.Resolve(singleServiceCallSite, new ServiceProviderScope(null, true));
            
            Assert.IsType<EnumerableService>(serviceObject);
            var service = (EnumerableService)serviceObject;
            Assert.NotNull(service.IntServices);
            Assert.Equal(2, service.IntServices.Count());
            Assert.All(service.IntServices, Assert.NotNull);
            Assert.Distinct(service.IntServices);
        }

        private FactoryCallSite BuildFactoryCallSite(Type serviceType, ServiceFactory factory)
        {
            var cacheInfo = new ServiceCacheInfo(ServiceLifetime.Transient, new ServiceIdentifier(serviceType));
            return new FactoryCallSite(cacheInfo, serviceType, factory);
        }
        
        [Fact]
        public void PrimaryFactoryCallSite()
        {
            var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());
            
            FactoryCallSite serviceCallSite = BuildFactoryCallSite(typeof(Service),
                provider => new Service(new IntService(), new StringService()));

            object serviceObject = resolver.Resolve(serviceCallSite, new ServiceProviderScope(null, true));
            Assert.IsType<Service>(serviceObject);
            var service = (Service)serviceObject;
            Assert.NotNull(service);
            Assert.NotNull(service.IntDep);
            Assert.NotNull(service.StringDep);
        }

        [Fact]
        public void SecondaryFactoryCallSite()
        {
            var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());

            FactoryCallSite intCallSite = BuildFactoryCallSite(typeof(IntService), provider => new IntService());
            FactoryCallSite stringCallSite = BuildFactoryCallSite(typeof(StringService), provider => new StringService());

            ConstructorCallSite serviceCallSite =
                BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });

            object serviceObject = resolver.Resolve(serviceCallSite, new ServiceProviderScope(null, true));
            Assert.IsType<Service>(serviceObject);
            var service = (Service)serviceObject;
            Assert.NotNull(service);
            Assert.NotNull(service.IntDep);
            Assert.NotNull(service.StringDep);
        }
    }
}