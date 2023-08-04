using System;

namespace DIContainer.Injector
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class InjectionAttribute : Attribute
    {
        
    }
}