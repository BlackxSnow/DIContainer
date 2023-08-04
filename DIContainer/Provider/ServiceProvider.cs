using System;
using System.Collections.Generic;
using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;
using DIContainer.NotableTypes;
using DIContainer.Provider.Engine;
using DIContainer.Provider.Temporary;
using DIContainer.Service;
using Microsoft.Extensions.Logging;

namespace DIContainer.Provider
{
    public class ServiceProvider : IServiceProvider
    {
        internal ServiceProviderScope RootScope { get; }
        internal InjectorCallSiteFactory InjectorCallSiteFactory { get; }
        internal CallSiteFactory CallSiteFactory { get; }
        internal NotableTypeFactory NotableTypeFactory { get; }

        private IServiceProviderEngine _Engine;
        private Dictionary<ServiceIdentifier, ServiceAccessor> _ServiceAccessors;

        private ILogger<ServiceProvider> _Logger;

        public TService? GetService<TService>()
        {
            return (TService?)GetService(typeof(TService));
        }

        public object? GetService(Type type)
        {
            return GetService(type, RootScope);
        }

        public object? GetService(Type type, ServiceProviderScope scope)
        {
            var identifier = new ServiceIdentifier(type);
            if (!_ServiceAccessors.TryGetValue(identifier, out ServiceAccessor accessor))
            {
                accessor = BuildServiceAccessor(identifier);
                _ServiceAccessors.Add(identifier, accessor);
            }

            return accessor.Resolver?.Invoke(scope);
        }
        
        public void AddTemporaryServices(ITemporaryServiceContainer container)
        {
            throw new NotImplementedException();
        }

        private void OnTemporaryContainerDisposed(ITemporaryServiceContainer container)
        {
            throw new NotImplementedException();
        }

        private ServiceAccessor BuildServiceAccessor(ServiceIdentifier identifier)
        {
            ServiceCallSite? callSite = CallSiteFactory.GetCallSite(identifier);
            if (callSite == null) return new ServiceAccessor { CallSite = callSite, Resolver = _ => null };

            if (callSite.CacheInfo.Location == CacheLocation.Root)
            {
                object? instance = CallSiteRuntimeResolver.Instance.Resolve(callSite, RootScope);
                return new ServiceAccessor() { CallSite = callSite, Resolver = _ => instance };
            }

            ServiceResolver resolver = _Engine.BuildResolver(callSite);
            return new ServiceAccessor() { CallSite = callSite, Resolver = resolver };
        }
        
        public ServiceProvider(IEnumerable<ServiceDescriptor> services, ILoggerFactory loggerFactory)
        {
            RootScope = new ServiceProviderScope(this, true);
            CallSiteFactory = new CallSiteFactory(this, services, loggerFactory.CreateLogger<CallSiteFactory>());
            InjectorCallSiteFactory = new InjectorCallSiteFactory(loggerFactory.CreateLogger<InjectorCallSiteFactory>(), CallSiteFactory);
            _ServiceAccessors = new Dictionary<ServiceIdentifier, ServiceAccessor>();

            _Engine = new RuntimeServiceProviderEngine();
            
            _Logger = loggerFactory.CreateLogger<ServiceProvider>();
            _Logger.LogInformation("Logger created");
        }

        public ServiceProvider(IEnumerable<ServiceDescriptor> services) : this(services,
            LoggerFactory.Create(b => b.AddConsole()))
        {
            
        }
        
    }
}