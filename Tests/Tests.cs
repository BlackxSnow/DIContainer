using System;
using System.Collections.Generic;
using System.Linq;
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

        private interface IEmptyServiceContainer
        {
            EmptyService Empty { get; }
        }
        
        private class SingleDependencyConstructorService : IEmptyServiceContainer
        {
            public EmptyService Empty { get; }
            
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
            var provider = new ServiceProvider(descriptors, GetLoggerFactory());
            var resolvedService = provider.GetService(typeof(EmptyService));
            Assert.NotNull(resolvedService);
        }
        
        [Fact]
        public void ResolveServiceMissingDependency()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(SingleDependencyConstructorService),
                    typeof(SingleDependencyConstructorService), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, GetLoggerFactory());
            Assert.Throws<InvalidOperationException>(() =>
                provider.GetService(typeof(SingleDependencyConstructorService)));
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
            var provider = new ServiceProvider(descriptors, GetLoggerFactory());
            var resolvedService = (SingleDependencyConstructorService)provider.GetService(typeof(SingleDependencyConstructorService));
            Assert.NotNull(resolvedService);
            Assert.NotNull(resolvedService.Empty);
        }

        [Fact]
        public void ResolveInterface()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IEmptyServiceContainer),
                    typeof(SingleDependencyConstructorService), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, GetLoggerFactory());
            var resolvedService = provider.GetService<IEmptyServiceContainer>();
            Assert.NotNull(resolvedService);
            Assert.NotNull(resolvedService.Empty);
        }
        
        [Fact]
        public void ResolveOpenGeneric()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(ILoggerFactory), typeof(LoggerFactory), ServiceLifetime.Singleton),
                new ServiceDescriptor(typeof(ILogger<>), typeof(Logger<>), ServiceLifetime.Singleton)
            };
            var provider = new ServiceProvider(descriptors, GetLoggerFactory());
            var logger = provider.GetService<ILogger<Tests>>();
            Assert.NotNull(logger);
        }

        [Fact]
        public void ResolveMultipleImplementations()
        {
            var descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IEmptyServiceContainer), 
                    typeof(SingleDependencyConstructorService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IEmptyServiceContainer), 
                    typeof(SingleDependencyConstructorService), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, GetLoggerFactory());
            var resolvedServices = provider.GetService<IEnumerable<IEmptyServiceContainer>>();
            Assert.NotNull(resolvedServices);
            Assert.Equal(2, resolvedServices.Count());
        }
    }
}