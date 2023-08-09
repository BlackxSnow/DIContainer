using System;
using System.Collections.Generic;

namespace DIContainer.Provider
{
    public class ServiceProviderScope : IServiceProviderScope, IServiceProvider
    {
        public bool IsRootScope { get; }
        internal readonly ServiceProvider RootProvider;
        private List<object> _DisposableObjects;
        private Dictionary<Type, object?> _ResolvedServices;
        
        public TService? GetService<TService>()
        {
            return RootProvider.GetService<TService>();
        }

        public object? GetService(Type type)
        {
            return RootProvider.GetService(type);
        }

        public ServiceProviderScope(ServiceProvider rootProvider, bool isRootScope)
        {
            RootProvider = rootProvider;
            _DisposableObjects = new List<object>();
            _ResolvedServices = new Dictionary<Type, object?>();
            IsRootScope = isRootScope;
        }
    }
}