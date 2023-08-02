using System;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    public class FactoryCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Factory;
        public override Type ServiceType { get; }
        public override Type? ImplementationType => null;

        public ServiceFactory Factory { get; }

        public FactoryCallSite(ServiceCacheInfo cacheInfo, Type serviceType, ServiceFactory factory) : base(cacheInfo)
        {
            ServiceType = serviceType;
            Factory = factory;
        }
    }
}