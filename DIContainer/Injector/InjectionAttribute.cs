using System;

namespace DIContainer.Injector
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class InjectionAttribute : Attribute
    {
        
    }
}