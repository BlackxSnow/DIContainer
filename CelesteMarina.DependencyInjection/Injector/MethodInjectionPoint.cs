using System.Reflection;
using CelesteMarina.DependencyInjection.CallSite;

namespace CelesteMarina.DependencyInjection.Injector
{
    /// <summary>
    /// Contains data for dependency injection through a non-constructor method.
    /// </summary>
    public class MethodInjectionPoint
    {
        public MethodInfo Method { get; }
        public ServiceCallSite[] ParameterCallSites { get; }

        public MethodInjectionPoint(MethodInfo method, ServiceCallSite[] parameterCallSites)
        {
            Method = method;
            ParameterCallSites = parameterCallSites;
        }
    }
}