using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CelesteMarina.DependencyInjection;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;
using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Service;
using CelesteMarina.DependencyInjection.Tests.Mock;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using IServiceProvider = CelesteMarina.DependencyInjection.Provider.IServiceProvider;

namespace CelesteMarina.DependencyInjection.Tests.Unit.CallSite
{
    public class CallSiteILResolverTests
    {
        private readonly ITestOutputHelper _TestOutputHelper;
        private readonly ILoggerFactory _LoggerFactory;

        public CallSiteILResolverTests(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
            _LoggerFactory = Utility.GetLoggerFactory(testOutputHelper);
        }



        private class IntService
        {
            public int Value { get; set; }
        }

        private class StringService
        {
            public string Value { get; set; }
        }

        private class EmptyService
        {
            
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

        private CallSiteILResolver BuildILResolver(IRootServiceProvider provider)
        {
            return new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>(), provider.RootScope,
                new CallSiteRuntimeResolver(_ => new RuntimeInjectorMock()), r => new InjectorILResolver(r));
        }
        
        private ConstructorCallSite BuildConstructorCallSite(Type serviceType, ServiceCallSite[] paramCallSites = null, 
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var cacheInfo = new ServiceCacheInfo(lifetime, new ServiceIdentifier(serviceType));
            ConstructorInfo constructor = serviceType.GetConstructors()[0];
            return new ConstructorCallSite(cacheInfo, null, serviceType, constructor,
                paramCallSites ?? Array.Empty<ServiceCallSite>());
        }

        [Fact]
        public void ConstantResolution()
        {
            var provider = new ServiceProviderMock();
            var resolverBuilder = BuildILResolver(provider);
            var callSite = new ConstantCallSite(typeof(Service), new Service(new IntService(), new StringService()));
            ServiceResolver resolver = resolverBuilder.Build(callSite);
            object serviceObject = resolver(provider.RootScope);

            Assert.NotNull(serviceObject);
            var service = Assert.IsType<Service>(serviceObject);
            Assert.NotNull(service.StringDep);
            Assert.NotNull(service.IntDep);
        }
        
        [Fact]
        public void PrimaryConstructorResolution()
        {
            var provider = new ServiceProviderMock();
            var resolverBuilder = BuildILResolver(provider);
        
            var intCallSite = new ConstantCallSite(typeof(IntService), new IntService());
            var stringCallSite = new ConstantCallSite(typeof(StringService), new StringService());
        
            ConstructorCallSite serviceCallSite =
                BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });

            ServiceResolver resolver = resolverBuilder.Build(serviceCallSite);
            object serviceObject = resolver(provider.RootScope);

            Assert.NotNull(serviceObject);
            var service = Assert.IsType<Service>(serviceObject);
            Assert.NotNull(service.StringDep);
            Assert.NotNull(service.IntDep);
        }
        
        [Fact]
        public void SecondaryConstructorResolution()
        {
            var provider = new ServiceProviderMock();
            var resolverBuilder = BuildILResolver(provider);
        
            var intCallSite = BuildConstructorCallSite(typeof(IntService));
            var stringCallSite = BuildConstructorCallSite(typeof(StringService));
        
            ConstructorCallSite serviceCallSite =
                BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });
        
            ServiceResolver resolver = resolverBuilder.Build(serviceCallSite);
            object serviceObject = resolver(provider.RootScope);

            Assert.NotNull(serviceObject);
            var service = Assert.IsType<Service>(serviceObject);
            Assert.NotNull(service.StringDep);
            Assert.NotNull(service.IntDep);
        }
        
        [Fact]
        public void PrimaryEnumerableResolution()
        {
            var provider = new ServiceProviderMock();
            var resolverBuilder = BuildILResolver(provider);
        
            var intCallSite = new ConstantCallSite(typeof(IntService), new IntService());
            var stringCallSite = new ConstantCallSite(typeof(StringService), new StringService());
        
            ConstructorCallSite singleServiceCallSite =
                BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });
        
            var enumerableCacheInfo = new ServiceCacheInfo(ServiceLifetime.Transient,
                new ServiceIdentifier(typeof(IEnumerable<Service>)));
            
            var enumerableCallSite = new EnumerableCallSite(enumerableCacheInfo, typeof(Service),
                new ServiceCallSite[] { singleServiceCallSite, singleServiceCallSite });
        
            ServiceResolver resolver = resolverBuilder.Build(enumerableCallSite);
            object serviceObjects = resolver(provider.RootScope);
            
            Service[] services = Assert.IsAssignableFrom<IEnumerable<Service>>(serviceObjects).ToArray();
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
        public void SecondaryEnumerableResolution()
        {
            var provider = new ServiceProviderMock();
            var resolverBuilder = BuildILResolver(provider);
        
            ConstructorCallSite intCallSite = BuildConstructorCallSite(typeof(IntService));
            var enumerableIntCacheInfo = new ServiceCacheInfo(ServiceLifetime.Transient,
                new ServiceIdentifier(typeof(IEnumerable<IntService>)));
            var intEnumerableCallSite = new EnumerableCallSite(enumerableIntCacheInfo, typeof(IntService),
                new ServiceCallSite[] { intCallSite, intCallSite });
            
            ConstructorCallSite singleServiceCallSite =
                BuildConstructorCallSite(typeof(EnumerableService), new ServiceCallSite[] { intEnumerableCallSite });

            ServiceResolver resolver = resolverBuilder.Build(singleServiceCallSite);
            object serviceObject = resolver(provider.RootScope);
            
            var service = Assert.IsType<EnumerableService>(serviceObject);
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
        public void PrimaryFactoryResolution()
        {
            var provider = new ServiceProviderMock();
            var resolverBuilder = BuildILResolver(provider);
            
            FactoryCallSite serviceCallSite = BuildFactoryCallSite(typeof(Service),
                p => new Service(new IntService(), new StringService()));
        
            ServiceResolver resolver = resolverBuilder.Build(serviceCallSite);
            object serviceObject = resolver(provider.RootScope);

            Assert.NotNull(serviceObject);
            var service = Assert.IsType<Service>(serviceObject);
            Assert.NotNull(service.StringDep);
            Assert.NotNull(service.IntDep);
        }
        
        [Fact]
        public void SecondaryFactoryResolution()
        {
            var provider = new ServiceProviderMock();
            var resolverBuilder = BuildILResolver(provider);
        
            FactoryCallSite intCallSite = BuildFactoryCallSite(typeof(IntService), p => new IntService());
            FactoryCallSite stringCallSite = BuildFactoryCallSite(typeof(StringService), p => new StringService());
        
            ConstructorCallSite serviceCallSite =
                BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });
        
            ServiceResolver resolver = resolverBuilder.Build(serviceCallSite);
            object serviceObject = resolver(provider.RootScope);

            Assert.NotNull(serviceObject);
            var service = Assert.IsType<Service>(serviceObject);
            Assert.NotNull(service.StringDep);
            Assert.NotNull(service.IntDep);
        }

        [Fact]
        public void ResolverCaching()
        {
            var resolverBuilder = BuildILResolver(new ServiceProviderMock());
        
            ConstructorCallSite callSite = BuildConstructorCallSite(typeof(EmptyService), 
                Array.Empty<ServiceCallSite>(), ServiceLifetime.Scoped);
            
            ServiceResolver firstResolver = resolverBuilder.Build(callSite);
            ServiceResolver secondResolver = resolverBuilder.Build(callSite);
            
            Assert.NotNull(firstResolver);
            Assert.NotNull(secondResolver);
            Assert.Same(firstResolver, secondResolver);
        }
        
        [Fact]
        public void PrimarySingletonResolution()
        {
            var provider = new ServiceProviderMock();
            var resolverBuilder = BuildILResolver(provider);
        
            ConstructorCallSite callSite = BuildConstructorCallSite(typeof(EmptyService), 
                Array.Empty<ServiceCallSite>(), ServiceLifetime.Singleton);
            
            ServiceResolver firstResolver = resolverBuilder.Build(callSite);
            ServiceResolver secondResolver = resolverBuilder.Build(callSite);
            
            object firstResolved = firstResolver(provider.RootScope);
            object secondResolved = secondResolver(provider.RootScope);
            
            Assert.NotNull(firstResolved);
            Assert.NotNull(secondResolved);
            Assert.Same(firstResolved, secondResolved);
        }
        
        [Fact]
        public void PrimaryScopedResolution()
        {
            var provider = new ServiceProviderMock();
            CallSiteILResolver resolverBuilder = BuildILResolver(provider);
        
            ConstructorCallSite callSite = BuildConstructorCallSite(typeof(EmptyService), 
                Array.Empty<ServiceCallSite>(), ServiceLifetime.Scoped);
            
            ServiceResolver resolver = resolverBuilder.Build(callSite);

            var scope = (ServiceProviderScope)provider.CreateScope();
            var differentScope = (ServiceProviderScope)provider.CreateScope();

            object firstObject = resolver(scope);
            object secondObject = resolver(scope);
            object differentObject = resolver(differentScope);
            
            Assert.NotNull(firstObject);
            Assert.NotNull(secondObject);
            Assert.NotNull(differentObject);
            Assert.Same(firstObject, secondObject);
            Assert.NotSame(firstObject, differentObject);
        }

        
        private class DisposableService : IDisposable
        {
            public bool IsDisposed { get; private set; }
            public void Dispose()
            {
                IsDisposed = true;
            }
        }
        
        [Fact]
        public void ScopedDispose()
        {
            var provider = new ServiceProviderMock();
            CallSiteILResolver resolverBuilder = BuildILResolver(provider);
        
            var firstScope = (ServiceProviderScope)provider.CreateScope();
            var secondScope = (ServiceProviderScope)provider.CreateScope();

            ConstructorCallSite callSite = BuildConstructorCallSite(typeof(DisposableService), 
                Array.Empty<ServiceCallSite>(), ServiceLifetime.Scoped);

            ServiceResolver resolver = resolverBuilder.Build(callSite);

            var firstResolved = (DisposableService)resolver(firstScope);
            var secondResolved = (DisposableService)resolver(secondScope);

            firstScope.Dispose();
            Assert.True(firstScope.IsDisposed);
            Assert.True(firstResolved?.IsDisposed);
            Assert.False(secondResolved?.IsDisposed);
        }
        
        [Fact]
        public void SingletonDispose()
        {
            var provider = new ServiceProviderMock();
            CallSiteILResolver resolverBuilder = BuildILResolver(provider);

            ConstructorCallSite callSite = BuildConstructorCallSite(typeof(DisposableService), 
                Array.Empty<ServiceCallSite>(), ServiceLifetime.Scoped);

            ServiceResolver resolver = resolverBuilder.Build(callSite);

            var resolved = (DisposableService)resolver(provider.RootScope);

            provider.RootScope.Dispose();
            Assert.True(provider.IsDisposed);
            Assert.True(provider.RootScope.IsDisposed);
            Assert.True(resolved?.IsDisposed);
        }
    }
}