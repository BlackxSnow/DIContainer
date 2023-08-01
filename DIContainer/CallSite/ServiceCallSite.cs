using System;
using DIContainer.Injector;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    public abstract class ServiceCallSite
    {
        public abstract Type? ServiceType { get; }
        public abstract Type? ImplementationType { get; }
        public abstract CallSiteKind Kind { get; }
        public InjectorCallSite? InjectorCallSite { get; }
        public bool IsTypeDisposable { get; }
        public ServiceCacheInfo CacheInfo { get; }
        public object? Value;
    }
}