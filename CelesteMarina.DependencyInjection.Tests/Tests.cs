using System;
using System.Collections.Generic;
using System.Linq;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Provider.Temporary;
using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.DependencyInjection.Logging;

namespace CelesteMarina.DependencyInjection.Tests
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
        
        private class EmptyServiceContainer : IEmptyServiceContainer
        {
            public EmptyService Empty { get; }
            
            public EmptyServiceContainer(EmptyService emptyService)
            {
                Empty = emptyService;
            }
        }

        [Fact]
        public void CreateEmptyProvider()
        {
            ServiceDescriptor[] descriptors = Array.Empty<ServiceDescriptor>();
            ServiceProvider provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
            Assert.NotNull(provider);
        }
        
        [Fact]
        public void CreateProvider()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(EmptyServiceContainer),
                    typeof(EmptyServiceContainer), ServiceLifetime.Transient)
            };
            ServiceProvider provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
            Assert.NotNull(provider);
        }
        
        [Fact]
        public void ResolveLeafService()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
            };
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
            var resolvedService = provider.GetService(typeof(EmptyService));
            Assert.NotNull(resolvedService);
        }
        
        [Fact]
        public void ResolveServiceMissingDependency()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyServiceContainer),
                    typeof(EmptyServiceContainer), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
            Assert.Throws<InvalidOperationException>(() =>
                provider.GetService(typeof(EmptyServiceContainer)));
        }
        
        [Fact]
        public void ResolveServiceWithDependency()
        {
            ServiceDescriptor[] descriptors = new[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(EmptyServiceContainer),
                    typeof(EmptyServiceContainer), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
            var resolvedService = (EmptyServiceContainer)provider.GetService(typeof(EmptyServiceContainer));
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
                    typeof(EmptyServiceContainer), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
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
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
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
                    typeof(EmptyServiceContainer), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IEmptyServiceContainer), 
                    typeof(EmptyServiceContainer), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
            var resolvedServices = provider.GetService<IEnumerable<IEmptyServiceContainer>>();
            Assert.NotNull(resolvedServices);
            Assert.Equal(2, resolvedServices.Count());
        }

        private class MethodInjectedService
        {
            public EmptyServiceContainer Empty;
            
            [Injection]
            public void Inject(EmptyServiceContainer single)
            {
                Empty = single;
            }
        }
        
        [Fact]
        public void ResolveWithMethodInjection()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(EmptyServiceContainer), typeof(EmptyServiceContainer), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(MethodInjectedService), typeof(MethodInjectedService), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
            var resolvedService = provider.GetService<MethodInjectedService>();
            Assert.NotNull(resolvedService);
            Assert.NotNull(resolvedService.Empty);
            Assert.NotNull(resolvedService.Empty.Empty);
        }
        
        private class PublicPropertyInjectedService
        {
            [Injection]
            public EmptyServiceContainer Empty { get; set; }
        }

        private class PrivateSetPropertyInjectedService
        {
            [Injection] public EmptyServiceContainer Empty { get; private set; }
        }
        private class PrivatePropertyInjectedService
        {
            [Injection] private EmptyServiceContainer _Empty { get; set; }
            public EmptyServiceContainer Empty => _Empty;
        }
        
        private class MultiplePropertyInjectedService
        {
            [Injection] private EmptyServiceContainer Empty { get; set; }
            public EmptyServiceContainer EmptyP => Empty;
            [Injection] public EmptyServiceContainer Empty2 { get; set; }
            [Injection] public EmptyServiceContainer Empty3 { get; private set; }
        }
        
        [Fact]
        public void ResolveWithPropertyInjection()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(EmptyServiceContainer), typeof(EmptyServiceContainer), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(PublicPropertyInjectedService), typeof(PublicPropertyInjectedService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(PrivateSetPropertyInjectedService), typeof(PrivateSetPropertyInjectedService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(PrivatePropertyInjectedService), typeof(PrivatePropertyInjectedService), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(MultiplePropertyInjectedService), typeof(MultiplePropertyInjectedService), ServiceLifetime.Transient)
            };
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));
            var pubService = provider.GetService<PublicPropertyInjectedService>();
            Assert.NotNull(pubService);
            Assert.NotNull(pubService.Empty);
            Assert.NotNull(pubService.Empty.Empty);
            
            var privService = provider.GetService<PrivateSetPropertyInjectedService>();
            Assert.NotNull(privService);
            Assert.NotNull(privService.Empty);
            Assert.NotNull(privService.Empty.Empty);
            
            var privProp = provider.GetService<PrivatePropertyInjectedService>();
            Assert.NotNull(privProp);
            Assert.NotNull(privProp.Empty);
            Assert.NotNull(privProp.Empty.Empty);
            
            var multiService = provider.GetService<MultiplePropertyInjectedService>();
            Assert.NotNull(multiService);
            Assert.NotNull(multiService.EmptyP);
            Assert.NotNull(multiService.EmptyP.Empty);
            Assert.NotNull(multiService.Empty2);
            Assert.NotNull(multiService.Empty2.Empty);
            Assert.NotNull(multiService.Empty3);
            Assert.NotNull(multiService.Empty3.Empty);
        }

        [Fact]
        public void TemporaryContainer()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(EmptyService), typeof(EmptyService), ServiceLifetime.Transient)
            };
            var tempContainer = new TemporaryServiceContainer(
                new ServiceDescriptor(typeof(EmptyServiceContainer), typeof(EmptyServiceContainer), ServiceLifetime.Transient));
            
            var provider = new ServiceProvider(descriptors, Utility.GetLoggerFactory(_TestOutputHelper));

            var containerPreAdd = provider.GetService<EmptyServiceContainer>();
            
            provider.AddTemporaryServices(tempContainer);

            var containerPostAdd = provider.GetService<EmptyServiceContainer>();
            
            tempContainer.Dispose();

            var containerDisposed = provider.GetService<EmptyServiceContainer>();
            
            Assert.Null(containerPreAdd);
            Assert.NotNull(containerPostAdd);
            Assert.Null(containerDisposed);
        }
    }
}