using System;

namespace DIContainer.Injector
{
    internal interface IInjectorCallSiteFactory
    {
        InjectorCallSite GetCallSite(Type type);
    }
}