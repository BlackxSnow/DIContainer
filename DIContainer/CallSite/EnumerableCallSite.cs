using System;
using System.Collections.Generic;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    public class EnumerableCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Enumerable;
        public override Type ServiceType { get; }
        public override Type? ImplementationType { get; }

        public Type SingleServiceType { get; }
        public ServiceCallSite[] CallSites { get; }

        public EnumerableCallSite(ServiceCacheInfo cacheInfo, Type innerType, ServiceCallSite[] callSites) :
            base(cacheInfo)
        {
            SingleServiceType = innerType;
            CallSites = callSites;
            ServiceType = typeof(IEnumerable<>).MakeGenericType(innerType);
            ImplementationType = innerType.MakeArrayType();
        }
    }
}