using System;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    public class EnumerableCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Enumerable;
        public override Type? ServiceType { get; }
        public override Type? ImplementationType { get; }

        public Type SingleServiceType { get; }
        public ServiceCallSite[] CallSites { get; }

        public EnumerableCallSite(ServiceCacheInfo cacheInfo) : base(cacheInfo)
        {
        }
    }
}