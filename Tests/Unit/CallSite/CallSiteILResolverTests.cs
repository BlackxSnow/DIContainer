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
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using IServiceProvider = DIContainer.Provider.IServiceProvider;

namespace Tests.Unit.CallSite
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

        public class InjectorMock : IInjectorRuntimeResolver
        {
            public void Inject(InjectorCallSite callSite, InjectorRuntimeResolverContext context)
            {
                
            }
        }

        public class ServiceProviderMock : IRootServiceProvider
        {
            public TService GetService<TService>() => throw new NotSupportedException();

            public object GetService(Type type) => throw new NotSupportedException();

            public Action Disposed { get; set; }
            public ServiceProviderScope RootScope { get; }
            public IServiceProviderScope CreateScope()
            {
                return new ServiceProviderScope(this, false);
            }

            public ServiceProviderMock()
            {
                RootScope = new ServiceProviderScope(this, true);
            }

            public void Dispose()
            {
                RootScope.Dispose();
                Disposed?.Invoke();
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
            var resolverBuilder = new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>());
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
            var resolverBuilder = new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>());
        
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
            var resolverBuilder = new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>());
        
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
            var resolverBuilder = new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>());
        
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
            var resolverBuilder = new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>());
        
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
        //
        // private FactoryCallSite BuildFactoryCallSite(Type serviceType, ServiceFactory factory)
        // {
        //     var cacheInfo = new ServiceCacheInfo(ServiceLifetime.Transient, new ServiceIdentifier(serviceType));
        //     return new FactoryCallSite(cacheInfo, serviceType, factory);
        // }
        //
        // [Fact]
        // public void PrimaryFactoryResolution()
        // {
        //     var provider = new ServiceProviderMock();
        //     var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());
        //     
        //     FactoryCallSite serviceCallSite = BuildFactoryCallSite(typeof(Service),
        //         p => new Service(new IntService(), new StringService()));
        //
        //     object serviceObject = resolver.Resolve(serviceCallSite, provider.RootScope);
        //     Assert.IsType<Service>(serviceObject);
        //     var service = (Service)serviceObject;
        //     Assert.NotNull(service);
        //     Assert.NotNull(service.IntDep);
        //     Assert.NotNull(service.StringDep);
        // }
        //
        // [Fact]
        // public void SecondaryFactoryResolution()
        // {
        //     var provider = new ServiceProviderMock();
        //     var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());
        //
        //     FactoryCallSite intCallSite = BuildFactoryCallSite(typeof(IntService), p => new IntService());
        //     FactoryCallSite stringCallSite = BuildFactoryCallSite(typeof(StringService), p => new StringService());
        //
        //     ConstructorCallSite serviceCallSite =
        //         BuildConstructorCallSite(typeof(Service), new ServiceCallSite[] { intCallSite, stringCallSite });
        //
        //     object serviceObject = resolver.Resolve(serviceCallSite, provider.RootScope);
        //     Assert.IsType<Service>(serviceObject);
        //     var service = (Service)serviceObject;
        //     Assert.NotNull(service);
        //     Assert.NotNull(service.IntDep);
        //     Assert.NotNull(service.StringDep);
        // }
        //
        //
        //
        // [Fact]
        // public void PrimarySingletonResolution()
        // {
        //     var provider = new ServiceProviderMock();
        //     var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());
        //
        //     ConstructorCallSite callSite = BuildConstructorCallSite(typeof(EmptyService), 
        //         Array.Empty<ServiceCallSite>(), ServiceLifetime.Singleton);
        //
        //     object firstResolved = resolver.Resolve(callSite, provider.RootScope);
        //     object secondResolved = resolver.Resolve(callSite, provider.RootScope);
        //     
        //     Assert.NotNull(firstResolved);
        //     Assert.NotNull(secondResolved);
        //     Assert.Same(firstResolved, secondResolved);
        // }
        //
        // [Fact]
        // public void PrimaryScopedResolution()
        // {
        //     var provider = new ServiceProviderMock();
        //     var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());
        //
        //     var scope = new ServiceProviderScope(provider, false);
        //     var secondScope = new ServiceProviderScope(provider, false);
        //
        //     ConstructorCallSite callSite = BuildConstructorCallSite(typeof(EmptyService), 
        //         Array.Empty<ServiceCallSite>(), ServiceLifetime.Scoped);
        //
        //     object firstResolved = resolver.Resolve(callSite, scope);
        //     object secondResolved = resolver.Resolve(callSite, scope);
        //     object differentScopeResolved = resolver.Resolve(callSite, secondScope);
        //     
        //     Assert.NotNull(firstResolved);
        //     Assert.NotNull(secondResolved);
        //     Assert.NotNull(differentScopeResolved);
        //     Assert.Same(firstResolved, secondResolved);
        //     Assert.NotSame(firstResolved, differentScopeResolved);
        // }
        //
        // private class DisposableService : IDisposable
        // {
        //     public bool IsDisposed { get; private set; }
        //     public void Dispose()
        //     {
        //         IsDisposed = true;
        //     }
        // }
        //
        // [Fact]
        // public void SingletonDispose()
        // {
        //     var provider = new ServiceProviderMock();
        //     var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());
        //
        //     var scope = new ServiceProviderScope(provider, false);
        //     var secondScope = new ServiceProviderScope(provider, false);
        //
        //     ConstructorCallSite callSite = BuildConstructorCallSite(typeof(DisposableService), 
        //         Array.Empty<ServiceCallSite>(), ServiceLifetime.Scoped);
        //
        //     var firstResolved = (DisposableService)resolver.Resolve(callSite, scope);
        //     var differentScopeResolved = (DisposableService)resolver.Resolve(callSite, secondScope);
        //     
        //     scope.Dispose();
        //     Assert.True(scope.IsDisposed);
        //     Assert.True(firstResolved?.IsDisposed);
        //     Assert.False(differentScopeResolved?.IsDisposed);
        // }
        //
        // [Fact]
        // public void ScopedDispose()
        // {
        //     var provider = new ServiceProviderMock();
        //     var resolver = new CallSiteRuntimeResolver(_ => new InjectorMock());
        //
        //     var scope = new ServiceProviderScope(provider, false);
        //     var secondScope = new ServiceProviderScope(provider, false);
        //
        //     ConstructorCallSite callSite = BuildConstructorCallSite(typeof(DisposableService), 
        //         Array.Empty<ServiceCallSite>(), ServiceLifetime.Scoped);
        //
        //     var firstResolved = (DisposableService)resolver.Resolve(callSite, scope);
        //     var differentScopeResolved = (DisposableService)resolver.Resolve(callSite, secondScope);
        //     
        //     scope.Dispose();
        //     Assert.True(scope.IsDisposed);
        //     Assert.True(firstResolved?.IsDisposed);
        //     Assert.False(differentScopeResolved?.IsDisposed);
        // }
    }
}