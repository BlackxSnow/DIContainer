using System;
using CelesteMarina.DependencyInjection.Service;
using CelesteMarina.DependencyInjection.Injector;

namespace CelesteMarina.DependencyInjection.CallSite
{
    /// <summary>
    /// Contains resolution data for a service implementation that is built via a factory.
    /// </summary>
    public class FactoryCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Factory;
        public override Type ServiceType { get; }
        public override Type? ImplementationType => null;

        public ServiceFactory Factory { get; }

        public FactoryCallSite(ServiceCacheInfo cacheInfo, Type serviceType, 
            ServiceFactory factory) : base(cacheInfo, null)
        {
            ServiceType = serviceType;
            Factory = factory;
        }
    }
}