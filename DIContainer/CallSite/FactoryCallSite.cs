using System;

namespace DIContainer.CallSite
{
    public class FactoryCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Factory;
        public override Type? ServiceType { get; }
        public override Type? ImplementationType { get; }

        public ServiceFactory Factory { get; }
    }
}