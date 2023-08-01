using System;

namespace DIContainer.CallSite
{
    public class ConstantCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Constant;
        public override Type? ServiceType { get; }
        public override Type? ImplementationType { get; }
    }
}