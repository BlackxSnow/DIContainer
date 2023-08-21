using System.Reflection;
using CelesteMarina.DependencyInjection.CallSite;

namespace CelesteMarina.DependencyInjection.Injector
{
    /// <summary>
    /// Contains data for dependency injection through a property.
    /// </summary>
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