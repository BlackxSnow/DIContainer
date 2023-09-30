using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;
using CelesteMarina.DependencyInjection.NotableTypes;
using CelesteMarina.DependencyInjection.Provider.Engine;
using CelesteMarina.DependencyInjection.Provider.Temporary;
using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Logging;

namespace CelesteMarina.DependencyInjection.Provider
{
    public class ServiceProvider : IRootServiceProvider
    {
        public event Action? Disposed;
        public bool IsDisposed { get; private set; }
        
        public ServiceProviderScope RootScope { get; }

        internal IInjectorCallSiteFactory InjectorCallSiteFactory { get; }
        internal ICallSiteFactory CallSiteFactory { get; }
        internal NotableTypeFactory NotableTypeFactory { get; }

        private IServiceProviderEngine _Engine;
        private HashSet<ITemporaryServiceContainer> _ActiveTemporaryContainers;
        private ConcurrentDictionary<ServiceIdentifier, ServiceAccessor> _ServiceAccessors;
        private ConcurrentDictionary<Type, ServiceInjectionAccessor> _ServiceInjectors;

        private ILogger? _Logger;


        public TService? GetService<TService>()
        {
            return (TService?)GetService(typeof(TService));
        }

        public object? GetService(Type type)
        {
            object? result = GetService(type, RootScope);
            if (result == null || type.IsInstanceOfType(result)) return result;
            throw new InvalidCastException();
        }

        public object? GetService(Type type, ServiceProviderScope scope)
        {
            var identifier = new ServiceIdentifier(type);
            _Logger?.LogTrace("GetService: {Type}", type);
            ServiceAccessor accessor = _ServiceAccessors.GetOrAdd(identifier, BuildServiceAccessor);
            return accessor.Resolver?.Invoke(scope);
        }

        public void InjectServices(object instance) => InjectServices(instance, RootScope);
        public void InjectServices(object instance, ServiceProviderScope scope)
        {
            Type targetType = instance.GetType();
            _Logger?.LogTrace("InjectServices: {Type} ({Instance})", targetType, instance);
            ServiceInjectionAccessor accessor = _ServiceInjectors.GetOrAdd(targetType, BuildServiceInjector);
            accessor.Injector.Invoke(scope, instance);
        }
        
        public void AddTemporaryServices(ITemporaryServiceContainer container)
        {
            _Logger?.LogDebug("Adding temporary service container");
            container.Disposed += OnTemporaryContainerDisposed;
            _ActiveTemporaryContainers.Add(container);
            CallSiteFactory.AddServices(container.Services);
            foreach (ServiceDescriptor descriptor in container.Services)
            {
                RemoveAccessor(descriptor);
            }
        }
        private void RemoveAccessor(ServiceDescriptor service)
        {
            _ServiceAccessors.TryRemove(new ServiceIdentifier(service), out _);
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

        private ServiceAccessor BuildServiceAccessor(ServiceIdentifier identifier)
        {
            using IDisposable? scope = _Logger?.BeginScope("Building ServiceAccessor for {IdentifierServiceType}",
                identifier.ServiceType);
            ServiceCallSite? callSite = CallSiteFactory.GetCallSite(identifier);
            if (callSite == null)
            {
                _Logger?.LogDebug("Service type does not exist - returning null accessor");
                return new ServiceAccessor { CallSite = callSite, Resolver = _ => null };
            }

            if (callSite.CacheInfo.Location == CacheLocation.Root)
            {
                _Logger?.LogTrace("Returning root instance accessor");
                object? instance = _Engine.RuntimeResolver.Resolve(callSite, RootScope);
                return new ServiceAccessor() { CallSite = callSite, Resolver = _ => instance };
            }

            _Logger?.LogTrace("Building resolver via engine");
            ServiceResolver resolver = _Engine.BuildResolver(callSite);
            return new ServiceAccessor() { CallSite = callSite, Resolver = resolver };
        }

        private ServiceInjectionAccessor BuildServiceInjector(Type targetType)
        {
            using IDisposable? scope = _Logger?.BeginScope("Building ServiceInjector for {TargetType}",
                targetType);
            InjectorCallSite callSite = InjectorCallSiteFactory.GetCallSite(targetType);
            
            _Logger?.LogTrace("Building injector via engine");
            return new ServiceInjectionAccessor(callSite, _Engine.BuildInjector(callSite));
        }
        
        internal void ReplaceServiceAccessor(ServiceCallSite callSite, ServiceResolver resolver)
        {
            _Logger?.LogDebug(
                "Replacing service accessor for {CallSiteServiceType}, Implementation: {CallSiteImplementationType}",
                callSite.ServiceType, callSite.ImplementationType);
            _ServiceAccessors[callSite.CacheInfo.CacheKey.Identifier] = new ServiceAccessor
            {
                CallSite = callSite,
                Resolver = resolver
            };
        }

        internal void ReplaceServiceInjector(InjectorCallSite callSite, ServiceInjector injector)
        {
            _Logger?.LogDebug("Replacing injector for {CallSiteTargetType}", callSite.TargetType);
            _ServiceInjectors[callSite.TargetType] = new ServiceInjectionAccessor(callSite, injector);
        }

        private void ReplaceInitialisationLoggers()
        {
            _Logger!.LogInformation("Discarding and attempting to replace initialisation logger");
            _Logger = GetService<ILogger<ServiceProvider>>();
            _Logger?.LogInformation("Successfully replaced initialisation logger");
            
            CallSiteFactory.OnInitialisationComplete(this);
            InjectorCallSiteFactory.OnInitialisationComplete(this);
            _Engine.OnInitialisationComplete(this);
        }
        
        public ServiceProvider(IEnumerable<ServiceDescriptor> services, ILoggerFactory loggerFactory)
        {
            _Logger = loggerFactory.CreateLogger<ServiceProvider>();
            
            RootScope = new ServiceProviderScope(this, true);
            
            var callSiteFactory = new CallSiteFactory(services, loggerFactory.CreateLogger<CallSiteFactory>());
            InjectorCallSiteFactory = new InjectorCallSiteFactory(loggerFactory.CreateLogger<InjectorCallSiteFactory>(), callSiteFactory);
            callSiteFactory.InjectorCallSiteFactory = InjectorCallSiteFactory;
            CallSiteFactory = callSiteFactory;
            
            callSiteFactory.AddService(new ServiceIdentifier(typeof(IServiceProvider)), new ServiceProviderCallSite());
            callSiteFactory.AddService(new ServiceIdentifier(typeof(IServiceProviderScopeFactory)),
                new ConstantCallSite(typeof(IServiceProviderScopeFactory), RootScope));
            
            _ServiceAccessors = new ConcurrentDictionary<ServiceIdentifier, ServiceAccessor>();
            _ServiceInjectors = new ConcurrentDictionary<Type, ServiceInjectionAccessor>();
            _ActiveTemporaryContainers = new HashSet<ITemporaryServiceContainer>();

            var runtimeResolver = new CallSiteRuntimeResolver(res => new InjectorRuntimeResolver(res));
            ILogger<CallSiteILResolver>? ilLogger = loggerFactory.CreateLogger<CallSiteILResolver>();
            IInjectorILResolver InjectorBuilder(ICallSiteILResolver r) => new InjectorILResolver(r);
            var ilResolver = new CallSiteILResolver(ilLogger, RootScope, runtimeResolver, InjectorBuilder);

            ILogger<DynamicServiceProviderEngine>? engineLogger = loggerFactory.CreateLogger<DynamicServiceProviderEngine>();
            _Engine = new DynamicServiceProviderEngine(runtimeResolver, ilResolver, this, engineLogger);
            ReplaceInitialisationLoggers();
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
            _ServiceAccessors = new ConcurrentDictionary<ServiceIdentifier, ServiceAccessor>();
            _ServiceInjectors = new ConcurrentDictionary<Type, ServiceInjectionAccessor>();
            _ActiveTemporaryContainers = new HashSet<ITemporaryServiceContainer>();
            CallSiteFactory = callSiteFactory;
            InjectorCallSiteFactory = injectorCallSiteFactory;
            _Engine = engine;
        }
    }
}