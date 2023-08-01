using System.Reflection;
using DIContainer.CallSite;

namespace DIContainer.Injector
{
    public struct PropertyInjectionPoint
    {
        public PropertyInfo Property { get; }
        public ServiceCallSite CallSite { get; }
    }
}