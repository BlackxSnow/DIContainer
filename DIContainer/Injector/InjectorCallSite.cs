using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DIContainer.Injector
{
    public class InjectorCallSite
    {
        public Type TargetType { get; }
        public MethodInjectionPoint? MethodInjectionPoint { get; }
        public PropertyInjectionPoint[]? PropertyInjectionPoints { get; }

        public InjectorCallSite(Type target, MethodInjectionPoint? methodPoint, PropertyInjectionPoint[]? propertyPoints)
        {
            TargetType = target;
            MethodInjectionPoint = methodPoint;
            PropertyInjectionPoints = propertyPoints;
        }
    }
}