using System;
using System.Collections.Generic;

namespace DIContainer.Provider
{
    public class ServiceProviderScope : IServiceProviderScope, IServiceProvider
    {
        private ServiceProvider _RootProvider;
        private List<object> _DisposableObjects;
        private Dictionary<Type, object?> _ResolvedServices;
        
        public TService GetService<TService>()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type type)
        {
            throw new NotImplementedException();
        }
    }
}