using System;
using System.Reflection;

namespace DIContainer.CallSite
{
    public class ConstructorCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Constructor;
        public override Type? ServiceType { get; }
        public override Type? ImplementationType { get; }

        public ConstructorInfo ConstructorInfo { get; }
        public ServiceCallSite[] ParameterCallSites { get; }
    }
}