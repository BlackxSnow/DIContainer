using System.Reflection;
using DIContainer.CallSite;

namespace DIContainer.Injector
{
    public struct MethodInjectionPoint
    {
        public MethodInfo Method { get; }
        public ServiceCallSite CallSite { get; } 
    }
}