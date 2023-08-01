using System;
using System.Collections.Generic;
using DIContainer.Provider;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    public class CallSiteFactory
    {
        private ServiceProvider _ServiceProvider;
        private Dictionary<ServiceIdentifier, List<ServiceDescriptor>> _Descriptors;
        private Dictionary<ServiceCacheKey, ServiceCallSite> _CallSiteCache;

        public ServiceCallSite? GetCallSite(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}