using System;

namespace DIContainer.Injector
{
    public class InjectorCallSite
    {
        public Type TargetType { get; }
        public MethodInjectionPoint? MethodInjectionPoint { get; }
        public PropertyInjectionPoint[]? PropertyInjectionPoints { get; }
    }
}