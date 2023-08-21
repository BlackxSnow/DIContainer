using System;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace CelesteMarina.DependencyInjection.Tests.Unit.CallSite
{
    public class CallSiteFactoryTests
    {
        private readonly ITestOutputHelper _TestOutputHelper;
        private readonly ILoggerFactory _LoggerFactory;
        
        public CallSiteFactoryTests(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
            _LoggerFactory = Utility.GetLoggerFactory(testOutputHelper);
        }

        private interface IServiceEmpty {}
        private class ServiceEmpty : IServiceEmpty {}

        private interface IService1Wide1Deep
        {
            IServiceEmpty EmptyService { get; }
        }

        private class Service1Wide1Deep : IService1Wide1Deep
        {
            public IServiceEmpty EmptyService { get; }

            public Service1Wide1Deep(IServiceEmpty empty)
            {
                EmptyService = empty;
            }
        }
        
        
        [Fact]
        public void ConstructWithServices()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(IServiceEmpty), typeof(ServiceEmpty), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IService1Wide1Deep), typeof(Service1Wide1Deep), ServiceLifetime.Transient)
            };
            var factory = new CallSiteFactory(descriptors, _LoggerFactory.CreateLogger<CallSiteFactory>());
            
            Assert.NotNull(factory);
            Assert.True(factory.IsService(typeof(IServiceEmpty)));
            Assert.True(factory.IsService(typeof(IService1Wide1Deep)));
        }

        [Fact]
        public void AddServices()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(IServiceEmpty), typeof(ServiceEmpty), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IService1Wide1Deep), typeof(Service1Wide1Deep), ServiceLifetime.Transient)
            };

            var factory = new CallSiteFactory(Array.Empty<ServiceDescriptor>(),
                _LoggerFactory.CreateLogger<CallSiteFactory>());
            
            factory.AddServices(descriptors);
            
            Assert.True(factory.IsService(typeof(IServiceEmpty)));
            Assert.True(factory.IsService(typeof(IService1Wide1Deep)));
        }
        
        
        [Fact]
        public void RemoveServices()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(IServiceEmpty), typeof(ServiceEmpty), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IService1Wide1Deep), typeof(Service1Wide1Deep), ServiceLifetime.Transient)
            };

            var factory = new CallSiteFactory(descriptors, _LoggerFactory.CreateLogger<CallSiteFactory>());
            
            factory.RemoveServices(descriptors);
            
            Assert.False(factory.IsService(typeof(IServiceEmpty)));
            Assert.False(factory.IsService(typeof(IService1Wide1Deep)));
        }
        
        [Fact]
        public void ReAddServices()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(IServiceEmpty), typeof(ServiceEmpty), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IService1Wide1Deep), typeof(Service1Wide1Deep), ServiceLifetime.Transient)
            };

            var factory = new CallSiteFactory(descriptors, _LoggerFactory.CreateLogger<CallSiteFactory>());
            
            factory.RemoveServices(descriptors);
            factory.AddServices(descriptors);
            
            Assert.True(factory.IsService(typeof(IServiceEmpty)));
            Assert.True(factory.IsService(typeof(IService1Wide1Deep)));
        }

        [Fact]
        public void BuildCallSite()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(IServiceEmpty), typeof(ServiceEmpty), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IService1Wide1Deep), typeof(Service1Wide1Deep), ServiceLifetime.Transient)
            };

            var factory = new CallSiteFactory(descriptors, _LoggerFactory.CreateLogger<CallSiteFactory>());

            ServiceCallSite? callSite = factory.GetCallSite(new ServiceIdentifier(typeof(IService1Wide1Deep)));
            
            Assert.NotNull(callSite);
            var constructorCallSite = Assert.IsType<ConstructorCallSite>(callSite);
            Assert.NotNull(constructorCallSite.ConstructorInfo);
            Assert.Equal(typeof(IService1Wide1Deep), callSite.ServiceType);
            Assert.Equal(typeof(Service1Wide1Deep), callSite.ImplementationType);
            
            ServiceCallSite emptyServiceCallSite = Assert.Single(constructorCallSite.ParameterCallSites);
            Assert.Equal(typeof(IServiceEmpty), emptyServiceCallSite.ServiceType);
            Assert.Equal(typeof(ServiceEmpty), emptyServiceCallSite.ImplementationType);
        }

        [Fact]
        public void RemoveServiceWithCallSite()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(IServiceEmpty), typeof(ServiceEmpty), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IService1Wide1Deep), typeof(Service1Wide1Deep), ServiceLifetime.Transient)
            };

            var factory = new CallSiteFactory(descriptors, _LoggerFactory.CreateLogger<CallSiteFactory>());

            ServiceCallSite? callSite = factory.GetCallSite(new ServiceIdentifier(typeof(IService1Wide1Deep)));
            
            factory.RemoveServices(descriptors);
            
            Assert.False(factory.IsService(typeof(IServiceEmpty)));
            Assert.False(factory.IsService(typeof(IService1Wide1Deep)));
            Assert.Null(factory.GetCallSite(new ServiceIdentifier(typeof(IServiceEmpty))));
            Assert.Null(factory.GetCallSite(new ServiceIdentifier(typeof(IService1Wide1Deep))));
            Assert.True(callSite?.IsDisabled);
        }
        
        [Fact]
        public void ReAddServiceWithCallSite()
        {
            var descriptors = new ServiceDescriptor[]
            {
                new ServiceDescriptor(typeof(IServiceEmpty), typeof(ServiceEmpty), ServiceLifetime.Transient),
                new ServiceDescriptor(typeof(IService1Wide1Deep), typeof(Service1Wide1Deep), ServiceLifetime.Transient)
            };

            var factory = new CallSiteFactory(descriptors, _LoggerFactory.CreateLogger<CallSiteFactory>());

            ServiceCallSite? callSite = factory.GetCallSite(new ServiceIdentifier(typeof(IService1Wide1Deep)));
            
            factory.RemoveServices(descriptors);
            factory.AddServices(descriptors);

            ServiceCallSite? readdedCallSite = factory.GetCallSite(new ServiceIdentifier(typeof(IService1Wide1Deep)));
            
            Assert.NotNull(factory.GetCallSite(new ServiceIdentifier(typeof(IServiceEmpty))));
            Assert.NotNull(readdedCallSite);
            Assert.Same(callSite, readdedCallSite);
            Assert.False(callSite?.IsDisabled);
        }
    }
}