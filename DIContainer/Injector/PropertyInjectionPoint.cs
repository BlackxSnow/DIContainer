using System.Reflection;
using DIContainer.CallSite;

namespace DIContainer.Injector
{
    public class PropertyInjectionPoint
    {
        public PropertyInfo Property { get; }
        public ServiceCallSite CallSite { get; }

        public PropertyInjectionPoint(PropertyInfo property, ServiceCallSite callSite)
        {
            Property = property;
            CallSite = callSite;
        }
    }
}