using System;
using System.Collections.Generic;
using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.Provider
{
    public class ServiceProviderScope : IServiceProviderScope, IServiceProvider, IServiceProviderScopeFactory
    {
        public bool IsRootScope { get; }
        internal readonly IRootServiceProvider RootProvider;
        private List<object> _DisposableObjects;
        public bool IsDisposed { get; private set; }
        internal Dictionary<ServiceCacheKey, object?> ResolvedServices;
        
        public TService? GetService<TService>()
        {
            return RootProvider.GetService<TService>();
        }

        public object? GetService(Type type)
        {
            return RootProvider.GetService(type);
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            RootProvider.Disposed -= Dispose;
            if (IsRootScope)
            {
                RootProvider.Dispose();
            }
            foreach (object obj in _DisposableObjects)
            {
                switch (obj)
                {
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable disposable:
                        disposable.DisposeAsync();
                        break;
                }
            }
        }

        public object? CaptureIfDisposable(object? instance)
        {
            if (instance is null or (not IDisposable and not IAsyncDisposable)) return instance;
            
            _DisposableObjects.Add(instance!);
            return instance;
        }
        
        public IServiceProviderScope CreateScope()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ServiceProviderScope));

            return RootProvider.CreateScope();
        }
        
        internal ServiceProviderScope(IRootServiceProvider rootProvider, bool isRootScope)
        {
            RootProvider = rootProvider;
            _DisposableObjects = new List<object>();
            ResolvedServices = new Dictionary<ServiceCacheKey, object?>();
            IsRootScope = isRootScope;
            rootProvider.Disposed += Dispose;
        }
    }
}