using System;

namespace DIContainer.CallSite
{
    public class EnumerableCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Enumerable;
        public override Type? ServiceType { get; }
        public override Type? ImplementationType { get; }

        public Type SingleServiceType { get; }
        public ServiceCallSite[] CallSites { get; }
    }
}