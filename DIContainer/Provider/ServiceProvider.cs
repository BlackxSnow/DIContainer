using System;
using System.Collections.Generic;
using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;
using DIContainer.NotableTypes;
using DIContainer.Provider.Engine;
using DIContainer.Provider.Temporary;
using DIContainer.Service;
using Microsoft.Extensions.Logging;

namespace DIContainer.Provider
{
    public class ServiceProvider : IRootServiceProvider
    {
        public event Action? Disposed;
        public ServiceProviderScope RootScope { get; }

        internal IInjectorCallSiteFactory InjectorCallSiteFactory { get; }
        internal ICallSiteFactory CallSiteFactory { get; }
        internal NotableTypeFactory NotableTypeFactory { get; }

        private IServiceProviderEngine _Engine;
        private HashSet<ITemporaryServiceContainer> _ActiveTemporaryContainers;
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

        private void RemoveAccessor(ServiceDescriptor service)
        {
            _ServiceAccessors.Remove(new ServiceIdentifier(service));
        }
        
        public void AddTemporaryServices(ITemporaryServiceContainer container)
        {
            container.Disposed += OnTemporaryContainerDisposed;
            _ActiveTemporaryContainers.Add(container);
            CallSiteFactory.AddServices(container.Services);
            foreach (ServiceDescriptor descriptor in container.Services)
            {
                RemoveAccessor(descriptor);
            }
        }

        private void OnTemporaryContainerDisposed(ITemporaryServiceContainer container)
        {
            if (!_ActiveTemporaryContainers.Contains(container)) return;

            CallSiteFactory.RemoveServices(container.Services);
            _ActiveTemporaryContainers.Remove(container);
            foreach (ServiceDescriptor descriptor in container.Services)
            {
                RemoveAccessor(descriptor);
            }
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
            if (IsDisposed) return;
            IsDisposed = true;
            RootScope.Dispose();
            Disposed?.Invoke();
        }
        
        public ServiceProvider(IEnumerable<ServiceDescriptor> services, ILoggerFactory loggerFactory)
        {
            _Logger = loggerFactory.CreateLogger<ServiceProvider>();
            
            RootScope = new ServiceProviderScope(this, true);
            var callSiteFactory = new CallSiteFactory(services, loggerFactory.CreateLogger<CallSiteFactory>());
            InjectorCallSiteFactory = new InjectorCallSiteFactory(loggerFactory.CreateLogger<InjectorCallSiteFactory>(), callSiteFactory);
            callSiteFactory.InjectorCallSiteFactory = InjectorCallSiteFactory;
            CallSiteFactory = callSiteFactory;
            
            _ServiceAccessors = new Dictionary<ServiceIdentifier, ServiceAccessor>();
            _ActiveTemporaryContainers = new HashSet<ITemporaryServiceContainer>();

            var runtimeResolver = new CallSiteRuntimeResolver(res => new InjectorRuntimeResolver(res));
            _Engine = new ILServiceProviderEngine(runtimeResolver, new CallSiteILResolver(
                loggerFactory.CreateLogger<CallSiteILResolver>(), RootScope, runtimeResolver,
                r => new InjectorILResolver(r)));

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
            _ActiveTemporaryContainers = new HashSet<ITemporaryServiceContainer>();
            CallSiteFactory = callSiteFactory;
            InjectorCallSiteFactory = injectorCallSiteFactory;
            _Engine = engine;
        }

    }
}