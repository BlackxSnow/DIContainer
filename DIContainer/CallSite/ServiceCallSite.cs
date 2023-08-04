using System;
using DIContainer.Injector;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    public abstract class ServiceCallSite
    {
        public abstract Type ServiceType { get; }
        public abstract Type? ImplementationType { get; }
        public abstract CallSiteKind Kind { get; }
        public InjectorCallSite? InjectorCallSite { get; }
        public ServiceCacheInfo CacheInfo { get; }
        public object? Value;

        public bool IsTypeDisposable => typeof(IDisposable).IsAssignableFrom(ImplementationType);

        public ServiceCallSite(ServiceCacheInfo cacheInfo, InjectorCallSite? injectorCallSite)
        {
            CacheInfo = cacheInfo;
            InjectorCallSite = injectorCallSite;
        }
    }
}