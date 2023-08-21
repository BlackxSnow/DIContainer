using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CelesteMarina.DependencyInjection;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;
using CelesteMarina.DependencyInjection.Tests.Mock;
using Microsoft.Extensions.Logging;
using CelesteMarina.DependencyInjection.Tests.Unit.CallSite;
using Xunit;
using Xunit.Abstractions;

namespace CelesteMarina.DependencyInjection.Tests.Unit.Injector
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
            [Injection] public string DependencyOne { get; private set; } = null!;
            public int DependencyTwo { get; private set; }

            [Injection]
            private void Inject(int depTwo)
            {
                DependencyTwo = depTwo;
            }

            public static InjectorCallSite GetInjectorCallSite()
            {
                PropertyInfo property = typeof(InjectedService).GetProperty(nameof(DependencyOne), Utility.AllInstance)!;
                MethodInfo method = typeof(InjectedService).GetMethod(nameof(Inject), Utility.AllInstance)!;

                var propertyPoint =
                    new PropertyInjectionPoint(property, _StringCallSite);
                var methodPoint = new MethodInjectionPoint(method, new ServiceCallSite[] { _IntCallSite });
                return new InjectorCallSite(typeof(InjectedService), methodPoint, new[] { propertyPoint });
            }
        }

        private const string TestString = "strDep";
        private const int TestInt = 7;

        private static readonly ConstantCallSite _IntCallSite = new(typeof(int), TestInt);
        private static readonly ConstantCallSite _StringCallSite = new(typeof(string), TestString);
        
        [Fact]
        public void BuildInjectorDelegate()
        {
            var callSiteResolver = new CallSiteILResolverMock(_IntCallSite, _StringCallSite);

            IInjectorILResolver injectorResolver = callSiteResolver.ILInjector;

            ServiceInjector injector = injectorResolver.BuildDelegate(InjectedService.GetInjectorCallSite());
            
            Assert.NotNull(injector);
        }
        
        [Fact]
        public void InjectViaDelegate()
        {
            var provider = new ServiceProviderMock();
            
            var callSiteResolver = new CallSiteILResolverMock(_IntCallSite, _StringCallSite);

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
            var callSiteResolver = new CallSiteILResolverMock(_IntCallSite, _StringCallSite);

            IInjectorILResolver injectorResolver = callSiteResolver.ILInjector;

            ServiceInjector firstInjector = injectorResolver.BuildDelegate(InjectedService.GetInjectorCallSite());
            ServiceInjector secondInjector = injectorResolver.BuildDelegate(InjectedService.GetInjectorCallSite());
            
            Assert.NotNull(firstInjector);
            Assert.NotNull(secondInjector);
            Assert.Same(firstInjector, secondInjector);
        }
    }
}