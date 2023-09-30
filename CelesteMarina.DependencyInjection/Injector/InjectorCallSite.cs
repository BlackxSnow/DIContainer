using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CelesteMarina.DependencyInjection.Injector
{
    /// <summary>
    /// Contains injection data for a target type.
    /// </summary>
    public class InjectorCallSite
    {
        /// <summary>
        /// The type handled by the call site.
        /// </summary>
        public Type TargetType { get; }
        /// <summary>
        /// The method injection point of the target type, if any.
        /// </summary>
        public MethodInjectionPoint? MethodInjectionPoint { get; }
        /// <summary>
        /// The property injection points of the target type, if any.
        /// </summary>
        public PropertyInjectionPoint[]? PropertyInjectionPoints { get; }

        public bool IsEmpty()
        {
            if (MethodInjectionPoint?.Method != null) return false;
            return PropertyInjectionPoints?.Any() == true;
        }
        
        public InjectorCallSite(Type target, MethodInjectionPoint? methodPoint, PropertyInjectionPoint[]? propertyPoints)
        {
            TargetType = target;
            MethodInjectionPoint = methodPoint;
            PropertyInjectionPoints = propertyPoints;
        }
    }
}