using System;
using System.Collections.Generic;
using DIContainer.CallSite;
using DIContainer.Injector;
using DIContainer.NotableTypes;
using DIContainer.Provider.Temporary;

namespace DIContainer.Provider
{
    public class ServiceProvider : IServiceProvider
    {
        internal ServiceProviderScope RootScope { get; }
        internal InjectorCallSiteFactory InjectorCallSiteFactory { get; }
        internal CallSiteFactory CallSiteFactory { get; }
        internal NotableTypeFactory NotableTypeFactory { get; }

        private IServiceProviderEngine _Engine;
        private Dictionary<Type, Func<ServiceProviderScope, object?>> _ServiceAccessors;

        public TService GetService<TService>()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type type)
        {
            throw new NotImplementedException();
        }

        public TService GetService<TService>(IServiceProviderScope scope)
        {
            throw new NotImplementedException();
        }
        
        public object GetService(Type type, IServiceProviderScope scope)
        {
            throw new NotImplementedException();
        }
        
        public void AddTemporaryServices(ITemporaryServiceContainer container)
        {
            throw new NotImplementedException();
        }

        private void OnTemporaryContainerDisposed(ITemporaryServiceContainer container)
        {
            throw new NotImplementedException();
        }
    }
}