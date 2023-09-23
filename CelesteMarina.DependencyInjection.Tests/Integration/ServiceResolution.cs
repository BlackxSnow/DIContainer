using System;
using CelesteMarina.DependencyInjection.Extensions;
using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Service;
using Xunit;
using IServiceProvider = CelesteMarina.DependencyInjection.Provider.IServiceProvider;

namespace CelesteMarina.DependencyInjection.Tests.Integration
{
    public class ServiceResolution
    {
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
    }
}