using System;
using CelesteMarina.DependencyInjection.Extensions;
using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using IServiceProvider = CelesteMarina.DependencyInjection.Provider.IServiceProvider;

namespace CelesteMarina.DependencyInjection.Tests.Integration
{
    public class ServiceResolution
    {
        private readonly ITestOutputHelper _TestOutputHelper;
        private readonly ILoggerFactory _LoggerFactory;

        public ServiceResolution(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
            _LoggerFactory = Utility.GetLoggerFactory(testOutputHelper);
        }
        
        [Fact]
        public void ResolveServiceProvider()
        {
            IServiceProvider provider = new ServiceProvider(Array.Empty<ServiceDescriptor>());
            Assert.NotNull(provider.GetService<IServiceProvider>());
        }
        
        [Fact]
        public void ResolveServiceProviderScopeFactory()
        {
            const int expected = 5;
            var services = new ServiceCollection();
            services.AddSingleton<int>((_) => expected);

            IServiceProvider provider = new ServiceProvider(services);

            var factory = provider.GetService<IServiceProviderScopeFactory>();

            Assert.NotNull(factory);
            IServiceProviderScope scope = factory.CreateScope();
            Assert.NotNull(scope);
            Assert.Equal(expected, scope.ServiceProvider.GetService<int>());
        }

        [Fact]
        public void ResolveInvalidTypeFactoryGeneric()
        {
            var services = new ServiceCollection();
            services.AddTransient<int>((_) => "Not an int");
            IServiceProvider provider = new ServiceProvider(services);

            var exception = Assert.ThrowsAny<Exception>(() => provider.GetService<int>());
            _TestOutputHelper.WriteLine(exception.ToString());
        }
        
        [Fact]
        public void ResolveInvalidTypeFactory()
        {
            var services = new ServiceCollection();
            services.AddTransient<int>((_) => "Not an int");
            IServiceProvider provider = new ServiceProvider(services);

            var exception = Assert.ThrowsAny<Exception>(() => provider.GetService(typeof(int)));
            _TestOutputHelper.WriteLine(exception.ToString());
        }

        private class Ref1 {}
        private class Ref2 {}
        
        private class SingleDependency
        {
            public Ref1 Dependency;

            public SingleDependency(Ref1 dependency)
            {
                Dependency = dependency;
            }
        }
        
        [Fact]
        public void ResolveInvalidTypeDependencyFactory()
        {
            var services = new ServiceCollection();
            services.AddTransient<Ref1>((_) => new Ref2());
            services.AddTransient<SingleDependency, SingleDependency>();
            IServiceProvider provider = new ServiceProvider(services);

            var exception = Assert.ThrowsAny<Exception>(() => provider.GetService(typeof(SingleDependency)));
            _TestOutputHelper.WriteLine(exception.ToString());
        }
        
        [Fact]
        public void ResolveWithMissingDependency()
        {
            var services = new ServiceCollection();
            services.AddTransient<SingleDependency, SingleDependency>();
            IServiceProvider provider = new ServiceProvider(services);

            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetService(typeof(SingleDependency)));
            _TestOutputHelper.WriteLine(exception.ToString());
        }
    }
}