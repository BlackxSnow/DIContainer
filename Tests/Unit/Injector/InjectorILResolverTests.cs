using System.Reflection;
using DIContainer;
using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;
using Microsoft.Extensions.Logging;
using Tests.Mock;
using Tests.Unit.CallSite;
using Xunit;
using Xunit.Abstractions;

namespace Tests.Unit.Injector
{
    public class InjectorILResolverTests
    {
        private readonly ITestOutputHelper _TestOutputHelper;
        private readonly ILoggerFactory _LoggerFactory;

        public InjectorILResolverTests(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
            _LoggerFactory = Utility.GetLoggerFactory(testOutputHelper);
        }

        private class InjectedService
        {
            [Injection] public string DependencyOne { get; private set; }
            public int DependencyTwo { get; private set; }

            [Injection]
            private void Inject(int depTwo)
            {
                DependencyTwo = depTwo;
            }

            public static InjectorCallSite GetInjectorCallSite()
            {
                PropertyInfo property = typeof(InjectedService).GetProperty(nameof(DependencyOne), Utility.AllInstance);
                MethodInfo method = typeof(InjectedService).GetMethod(nameof(Inject), Utility.AllInstance);

                var propertyPoint =
                    new PropertyInjectionPoint(property, new ConstantCallSite(typeof(string), "strDep"));
                var methodPoint = new MethodInjectionPoint(method,
                    new ServiceCallSite[] { new ConstantCallSite(typeof(int), 7) });
                return new InjectorCallSite(typeof(InjectedService), methodPoint, new[] { propertyPoint });
            }
        }
        
         
        
        [Fact]
        public void BuildInjectorDelegate()
        {
            var provider = new ServiceProviderMock();
            
            var callSiteResolver = new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>(),
                provider.RootScope, new CallSiteRuntimeResolver(r => new RuntimeInjectorMock()),
                r => new InjectorILResolver(r));
            
            IInjectorILResolver injectorResolver = callSiteResolver.ILInjector;

            ServiceInjector injector = injectorResolver.BuildDelegate(InjectedService.GetInjectorCallSite());
            
            Assert.NotNull(injector);
        }
        
        [Fact]
        public void InjectViaDelegate()
        {
            var provider = new ServiceProviderMock();
            
            var callSiteResolver = new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>(),
                provider.RootScope, new CallSiteRuntimeResolver(r => new RuntimeInjectorMock()),
                r => new InjectorILResolver(r));
            
            IInjectorILResolver injectorResolver = callSiteResolver.ILInjector;

            ServiceInjector injector = injectorResolver.BuildDelegate(InjectedService.GetInjectorCallSite());

            var service = new InjectedService();

            injector(provider.RootScope, service);
            
            Assert.NotNull(service);
            Assert.Equal("strDep", service.DependencyOne);
            Assert.Equal(7, service.DependencyTwo);
        }
        
        [Fact]
        public void InjectorCaching()
        {
            var provider = new ServiceProviderMock();
            
            var callSiteResolver = new CallSiteILResolver(_LoggerFactory.CreateLogger<CallSiteILResolver>(),
                provider.RootScope, new CallSiteRuntimeResolver(r => new RuntimeInjectorMock()),
                r => new InjectorILResolver(r));
            
            IInjectorILResolver injectorResolver = callSiteResolver.ILInjector;

            ServiceInjector firstInjector = injectorResolver.BuildDelegate(InjectedService.GetInjectorCallSite());
            ServiceInjector secondInjector = injectorResolver.BuildDelegate(InjectedService.GetInjectorCallSite());
            
            Assert.NotNull(firstInjector);
            Assert.NotNull(secondInjector);
            Assert.Same(firstInjector, secondInjector);
        }
    }
}