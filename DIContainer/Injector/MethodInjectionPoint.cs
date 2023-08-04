using System.Reflection;
using DIContainer.CallSite;

namespace DIContainer.Injector
{
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