using System;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;
using CelesteMarina.DependencyInjection.Provider;
using Xunit;
using Xunit.Abstractions;

namespace CelesteMarina.DependencyInjection.Tests.Unit.Injector
{
    public class InjectorRuntimeResolverTests
    {
        private readonly ITestOutputHelper _TestOutputHelper;

        public InjectorRuntimeResolverTests(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
        }

        private class CallSiteResolver : ICallSiteRuntimeResolver
        {
            public IInjectorRuntimeResolver RuntimeInjector { get; }

            public object Resolve(ServiceCallSite callSite, ServiceProviderScope scope)
            {
                if (callSite is ConstantCallSite constant) return constant.Value;
                throw new InvalidOperationException("Resolver mockup only supports ConstantCallSite");
            }
        }  
        
        private class InjectedService
        {
            [Injection]
            public string StringDependency { get; set; }

            public int IntDependency;

            [Injection]
            public void Inject(int idep)
            {
                IntDependency = idep;
            }
        }

        private ServiceCallSite BuildStringCallSite(string value)
        {
            return new ConstantCallSite(typeof(string), value);
        }

        private ServiceCallSite BuildIntCallSite(int value)
        {
            return new ConstantCallSite(typeof(int), value);
        }
        
        [Fact]
        public void PropertyInjection()
        {
            const string stringValue = "string value";

            var callSite = new InjectorCallSite(typeof(InjectedService), null, new[]
            {
                new PropertyInjectionPoint(
                    typeof(InjectedService).GetProperty(nameof(InjectedService.StringDependency)) ??
                    throw new InvalidOperationException(), BuildStringCallSite(stringValue))
            });

            var service = new InjectedService();
            var injector = new InjectorRuntimeResolver(new CallSiteResolver());
            injector.Inject(callSite, new InjectorRuntimeResolverContext(null, service));
            Assert.Equal(stringValue, service.StringDependency);
        }
        
        [Fact]
        public void MethodInjection()
        {
            const int intValue = 7;

            var methodInjectionPoint = new MethodInjectionPoint(
                typeof(InjectedService).GetMethod(nameof(InjectedService.Inject)), new ServiceCallSite[]
                {
                    new ConstantCallSite(typeof(int), intValue)
                });

            var callSite = new InjectorCallSite(typeof(InjectedService), methodInjectionPoint, null);

            var service = new InjectedService();
            var injector = new InjectorRuntimeResolver(new CallSiteResolver());
            injector.Inject(callSite, new InjectorRuntimeResolverContext(null, service));
            Assert.Equal(intValue, service.IntDependency);
        }
    }
}