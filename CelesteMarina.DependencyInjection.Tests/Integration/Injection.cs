using System;
using CelesteMarina.DependencyInjection.Extensions;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace CelesteMarina.DependencyInjection.Tests.Integration
{
    public class Injection
    {
        private readonly ITestOutputHelper _TestOutputHelper;
        private readonly ILoggerFactory _LoggerFactory;

        public Injection(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
            _LoggerFactory = Utility.GetLoggerFactory(testOutputHelper);
        }

        
        private class D1 {}
        private class D2 {}
        private class D3 {}
        
        private class ResolveAndInjectTarget
        {
            [Injection]
            public D1 PropertyInjected { get; private set; } = null!;

            public D2 MethodInjected { get; private set; } = null!;
            public D3 Resolved { get; }

            [Injection]
            private void Inject(D2 d2)
            {
                MethodInjected = d2;
            }
            
            public ResolveAndInjectTarget(D3 d3)
            {
                Resolved = d3;
            }
        }
        
        [Fact]
        public void ResolveAndInject_Both()
        {
            var services = new ServiceCollection();
            services.AddTransient<D1, D1>();
            services.AddTransient<D2, D2>();
            services.AddTransient<D3, D3>();
            services.AddTransient<ResolveAndInjectTarget, ResolveAndInjectTarget>();
            var provider = new ServiceProvider(services);

            var resolved = provider.GetService<ResolveAndInjectTarget>();

            Assert.NotNull(resolved);
            Assert.NotNull(resolved.PropertyInjected);
            Assert.NotNull(resolved.MethodInjected);
            Assert.NotNull(resolved.Resolved);
        }
        
        [Fact]
        public void ResolveAndInject_MissingProperty()
        {
            var services = new ServiceCollection();
            services.AddTransient<D2, D2>();
            services.AddTransient<D3, D3>();
            services.AddTransient<ResolveAndInjectTarget, ResolveAndInjectTarget>();
            var provider = new ServiceProvider(services);

            var exception = Assert.Throws<InvalidOperationException>(provider.GetService<ResolveAndInjectTarget>);
            _TestOutputHelper.WriteLine(exception.ToString());
        }
        
        [Fact]
        public void ResolveAndInject_MissingMethodParameter()
        {
            var services = new ServiceCollection();
            services.AddTransient<D1, D1>();
            services.AddTransient<D3, D3>();
            services.AddTransient<ResolveAndInjectTarget, ResolveAndInjectTarget>();
            var provider = new ServiceProvider(services);

            var exception = Assert.Throws<InvalidOperationException>(provider.GetService<ResolveAndInjectTarget>);
            _TestOutputHelper.WriteLine(exception.ToString());
        }
    }
}