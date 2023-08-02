using System;
using DIContainer.Provider;
using DIContainer.Service;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.DependencyInjection.Logging;

namespace Tests
{
    public class Tests
    {
        private readonly ITestOutputHelper _TestOutputHelper;

        public Tests(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
        }

        private class EmptyService
        {
            
        }

        private class SingleDependencyConstructorService
        {
            public EmptyService Empty;
            
            public SingleDependencyConstructorService(EmptyService emptyService)
            {
                Empty = emptyService;
            }
        }

        private ILoggerFactory GetLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(_TestOutputHelper);
                builder.AddFilter("*", LogLevel.Trace);
            });
        }
        
        [Fact]
        public void CreateEmptyProvider()
        {
            ServiceDescriptor[] descriptors = Array.Empty<ServiceDescriptor>();
            ServiceProvider provider = new ServiceProvider(descriptors, GetLoggerFactory());
            Assert.NotNull(provider);
        }
        
        [Fact]
        public void CreateProvider()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(SingleDependencyConstructorService),
                    typeof(SingleDependencyConstructorService), ServiceLifetime.Transient)
            };
            ServiceProvider provider = new ServiceProvider(descriptors, GetLoggerFactory());
            Assert.NotNull(provider);
        }
        
        [Fact]
        public void ResolveLeafService()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
            };
            ServiceProvider provider = new ServiceProvider(descriptors, GetLoggerFactory());
            var resolvedService = provider.GetService(typeof(EmptyService));
            Assert.NotNull(resolvedService);
        }
        
        [Fact]
        public void ResolveServiceWithDependency()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(SingleDependencyConstructorService),
                    typeof(SingleDependencyConstructorService), ServiceLifetime.Transient)
            };
            ServiceProvider provider = new ServiceProvider(descriptors, GetLoggerFactory());
            var resolvedService = (SingleDependencyConstructorService)provider.GetService(typeof(SingleDependencyConstructorService));
            Assert.NotNull(resolvedService);
            Assert.NotNull(resolvedService.Empty);
        }
    }
}