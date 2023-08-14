using System;

namespace DIContainer.Injector
{
    /// <summary>
    /// Marks properties and methods for post-construction dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class InjectionAttribute : Attribute
    {
        
    }
}