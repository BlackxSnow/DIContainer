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
    public class ServiceProvider : IRootServiceProvider
    {
        public Action Disposed { get; set; }
        public ServiceProviderScope RootScope { get; }

        internal IInjectorCallSiteFactory InjectorCallSiteFactory { get; }
        internal ICallSiteFactory CallSiteFactory { get; }
        internal NotableTypeFactory NotableTypeFactory { get; }

        private IServiceProviderEngine _Engine;
        private Dictionary<ServiceIdentifier, ServiceAccessor> _ServiceAccessors;

        private ILogger<ServiceProvider> _Logger;

        public bool IsDisposed { get; private set; }

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
                object? instance = _Engine.RuntimeResolver.Resolve(callSite, RootScope);
                return new ServiceAccessor() { CallSite = callSite, Resolver = _ => instance };
            }

            ServiceResolver resolver = _Engine.BuildResolver(callSite);
            return new ServiceAccessor() { CallSite = callSite, Resolver = resolver };
        }
        
        public IServiceProviderScope CreateScope()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ServiceProvider));

            return new ServiceProviderScope(this, false);
        }
        
        public void Dispose()
        {
            IsDisposed = true;
            RootScope.Dispose();
            Disposed?.Invoke();
        }
        
        public ServiceProvider(IEnumerable<ServiceDescriptor> services, ILoggerFactory loggerFactory)
        {
            RootScope = new ServiceProviderScope(this, true);
            var callSiteFactory = new CallSiteFactory(services, loggerFactory.CreateLogger<CallSiteFactory>());
            InjectorCallSiteFactory = new InjectorCallSiteFactory(loggerFactory.CreateLogger<InjectorCallSiteFactory>(), callSiteFactory);
            callSiteFactory.InjectorCallSiteFactory = InjectorCallSiteFactory;
            CallSiteFactory = callSiteFactory;
            
            _ServiceAccessors = new Dictionary<ServiceIdentifier, ServiceAccessor>();

            _Engine = new RuntimeServiceProviderEngine();
            
            _Logger = loggerFactory.CreateLogger<ServiceProvider>();
        }
        
        public ServiceProvider(IEnumerable<ServiceDescriptor> services) : this(services,
            LoggerFactory.Create(b => b.AddConsole()))
        {
            
        }

        internal ServiceProvider(ILoggerFactory loggerFactory, ICallSiteFactory callSiteFactory,
            IInjectorCallSiteFactory injectorCallSiteFactory, IServiceProviderEngine engine)
        {
            _Logger = loggerFactory.CreateLogger<ServiceProvider>();
            
            RootScope = new ServiceProviderScope(this, true);
            _ServiceAccessors = new Dictionary<ServiceIdentifier, ServiceAccessor>();
            CallSiteFactory = callSiteFactory;
            InjectorCallSiteFactory = injectorCallSiteFactory;
            _Engine = engine;
        }

    }
}