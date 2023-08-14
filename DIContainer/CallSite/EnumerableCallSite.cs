using System;
using System.Collections.Generic;
using DIContainer.Injector;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    /// Contains resolution data for all valid implementations of a service.
    public class EnumerableCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Enumerable;
        public override Type ServiceType { get; }
        public override Type? ImplementationType { get; }
        /// <summary>
        /// The non-enumerable type of the service.
        /// </summary>
        public Type SingleServiceType { get; }
        /// <summary>
        /// The child call sites for all valid SingleServiceType implementations.
        /// </summary>
        public ServiceCallSite[] CallSites { get; }

        public EnumerableCallSite(ServiceCacheInfo cacheInfo, Type innerType,
            ServiceCallSite[] callSites) : base(cacheInfo, null)
        {
            SingleServiceType = innerType;
            CallSites = callSites;
            ServiceType = typeof(IEnumerable<>).MakeGenericType(innerType);
            ImplementationType = innerType.MakeArrayType();
        }
    }
}