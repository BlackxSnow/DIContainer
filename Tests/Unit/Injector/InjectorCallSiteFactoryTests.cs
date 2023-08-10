using System;
using System.Reflection;
using DIContainer.CallSite;
using DIContainer.Injector;
using DIContainer.Service;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Tests.Unit.Injector
{
    public class InjectorCallSiteFactoryTests
    {
        private readonly ITestOutputHelper _TestOutputHelper;
        private readonly ILoggerFactory _LoggerFactory;

        public InjectorCallSiteFactoryTests(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
            _LoggerFactory = Utility.GetLoggerFactory(testOutputHelper);
        }

        private class CallSiteFactory : ICallSiteFactory
        {
            private ServiceCallSite IntCallSite;
            private ServiceCallSite StringCallSite;
            
            public ServiceCallSite GetCallSite(ServiceIdentifier identifier)
            {
                if (identifier.ServiceType == typeof(int)) return IntCallSite;
                if (identifier.ServiceType == typeof(string)) return StringCallSite;

                throw new InvalidOperationException("Only string and int are supported by this mock class");
            }

            public CallSiteFactory(int iVal, string sVal)
            {
                IntCallSite = new ConstantCallSite(typeof(int), iVal);
                StringCallSite = new ConstantCallSite(typeof(string), sVal);
            }
        }

        private class PublicPropertyService
        {
            [Injection]
            public string StringDependency { get; set; }

            public static PropertyInfo GetPropertyInfo()
            {
                return typeof(PublicPropertyService).GetProperty(nameof(StringDependency), Utility.AllInstance);
            }
        }

        [Fact] 
        public void PublicPropertyCallSite()
        {
            const int intVal = 7;
            const string strVal = "string";
            var factory = new InjectorCallSiteFactory(_LoggerFactory.CreateLogger<InjectorCallSiteFactory>(),
                new CallSiteFactory(intVal, strVal));
            InjectorCallSite callSite = factory.GetCallSite(typeof(PublicPropertyService));
            
            PropertyInjectionPoint point = callSite.PropertyInjectionPoints?[0];
            
            Assert.Null(callSite.MethodInjectionPoint);
            Assert.Equal(1, callSite.PropertyInjectionPoints?.Length);
            Assert.Equal(PublicPropertyService.GetPropertyInfo(), point?.Property);
            Assert.NotNull(point?.CallSite);
        }
        
        private class PrivatePropertyService
        {
            [Injection]
            private string StringDependency { get; set; }

            public static PropertyInfo GetPropertyInfo()
            {
                return typeof(PrivatePropertyService).GetProperty(nameof(StringDependency), Utility.AllInstance);
            }
        }
        
        [Fact] 
        public void PrivatePropertyCallSite()
        {
            const int intVal = 7;
            const string strVal = "string";
            var factory = new InjectorCallSiteFactory(_LoggerFactory.CreateLogger<InjectorCallSiteFactory>(),
                new CallSiteFactory(intVal, strVal));
            InjectorCallSite callSite = factory.GetCallSite(typeof(PrivatePropertyService));
            
            PropertyInjectionPoint point = callSite.PropertyInjectionPoints?[0];
            
            Assert.Null(callSite.MethodInjectionPoint);
            Assert.Equal(1, callSite.PropertyInjectionPoints?.Length);
            Assert.Equal(PrivatePropertyService.GetPropertyInfo(), point?.Property);
            Assert.NotNull(point?.CallSite);
        }
        
        private class MultiPropertyService
        {
            [Injection]
            public string StringDependency { get; set; }
            [Injection]
            public int IntDependency { get; set; }

            public static PropertyInfo[] GetPropertyInfo()
            {
                return typeof(MultiPropertyService).GetProperties(Utility.AllInstance);
            }
        }
        
        [Fact] 
        public void MultiPropertyCallSite()
        {
            const int intVal = 7;
            const string strVal = "string";
            var factory = new InjectorCallSiteFactory(_LoggerFactory.CreateLogger<InjectorCallSiteFactory>(),
                new CallSiteFactory(intVal, strVal));
            InjectorCallSite callSite = factory.GetCallSite(typeof(MultiPropertyService));
            
            PropertyInjectionPoint point1 = callSite.PropertyInjectionPoints?[0];
            PropertyInjectionPoint point2 = callSite.PropertyInjectionPoints?[1];
            PropertyInfo[] expectedProperties = MultiPropertyService.GetPropertyInfo();
            
            Assert.Null(callSite.MethodInjectionPoint);
            Assert.Equal(2, callSite.PropertyInjectionPoints?.Length);
            Assert.Equal(expectedProperties[0], point1?.Property);
            Assert.Equal(expectedProperties[1], point2?.Property);
            Assert.NotNull(point1?.CallSite);
            Assert.NotNull(point2?.CallSite);
        }
        
        private class NonInjectionPropertiesService
        {
            [Injection]
            public string StringDependency { get; set; }
            
            public int someOtherValue { get; set; }

            public static PropertyInfo GetPropertyInfo()
            {
                return typeof(NonInjectionPropertiesService).GetProperty(nameof(StringDependency), Utility.AllInstance);
            }
        }
        
        [Fact] 
        public void NonInjectionPropertyCallSite()
        {
            const int intVal = 7;
            const string strVal = "string";
            var factory = new InjectorCallSiteFactory(_LoggerFactory.CreateLogger<InjectorCallSiteFactory>(),
                new CallSiteFactory(intVal, strVal));
            InjectorCallSite callSite = factory.GetCallSite(typeof(NonInjectionPropertiesService));
            
            PropertyInjectionPoint point = callSite.PropertyInjectionPoints?[0];
            
            Assert.Null(callSite.MethodInjectionPoint);
            Assert.Equal(1, callSite.PropertyInjectionPoints?.Length);
            Assert.Equal(NonInjectionPropertiesService.GetPropertyInfo(), point?.Property);
            Assert.NotNull(point?.CallSite);
        }

        private class PublicMethodInjectedService
        {
            [Injection]
            public void Inject(string sDep)
            {
                
            }

            public static MethodInfo GetInjectMethodInfo()
            {
                return typeof(PublicMethodInjectedService).GetMethod(nameof(Inject), Utility.AllInstance);
            }
        }
        
        [Fact] 
        public void PublicMethodCallSite()
        {
            const int intVal = 7;
            const string strVal = "string";
            var factory = new InjectorCallSiteFactory(_LoggerFactory.CreateLogger<InjectorCallSiteFactory>(),
                new CallSiteFactory(intVal, strVal));
            InjectorCallSite callSite = factory.GetCallSite(typeof(PublicMethodInjectedService));

            Assert.Null(callSite.PropertyInjectionPoints);
            Assert.NotNull(callSite.MethodInjectionPoint);
            Assert.Equal(PublicMethodInjectedService.GetInjectMethodInfo(), callSite.MethodInjectionPoint.Method);
            Assert.Equal(1, callSite.MethodInjectionPoint.ParameterCallSites?.Length);
            Assert.NotNull(callSite.MethodInjectionPoint.ParameterCallSites[0]);
        }
        
        private class PrivateMethodInjectedService
        {
            [Injection]
            private void Inject(string sDep)
            {
                
            }

            public static MethodInfo GetInjectMethodInfo()
            {
                return typeof(PrivateMethodInjectedService).GetMethod(nameof(Inject), Utility.AllInstance);
            }
        }
        
        [Fact] 
        public void PrivateMethodCallSite()
        {
            const int intVal = 7;
            const string strVal = "string";
            var factory = new InjectorCallSiteFactory(_LoggerFactory.CreateLogger<InjectorCallSiteFactory>(),
                new CallSiteFactory(intVal, strVal));
            InjectorCallSite callSite = factory.GetCallSite(typeof(PrivateMethodInjectedService));

            Assert.Null(callSite.PropertyInjectionPoints);
            Assert.NotNull(callSite.MethodInjectionPoint);
            Assert.Equal(PrivateMethodInjectedService.GetInjectMethodInfo(), callSite.MethodInjectionPoint.Method);
            Assert.Equal(1, callSite.MethodInjectionPoint.ParameterCallSites?.Length);
            Assert.NotNull(callSite.MethodInjectionPoint.ParameterCallSites[0]);
        }
    }
}