using System;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.CallSite
{
    /// <summary>
    /// Contains resolution data for an implementation of a service.
    /// </summary>
    public abstract class ServiceCallSite
    {
        /// <summary>
        /// Whether the service represented by the call site is disabled.
        /// </summary>
        public bool IsDisabled { get; internal set; }
        /// <summary>
        /// The type of service fulfilled by the call site.
        /// </summary>
        public abstract Type ServiceType { get; }
        /// <summary>
        /// The type of object that fulfills requests for the ServiceType.
        /// </summary>
        public abstract Type? ImplementationType { get; }
        /// <summary>
        /// The resolution kind of the call site.
        /// </summary>
        public abstract CallSiteKind Kind { get; }
        /// <summary>
        /// The InjectorCallSite for the ImplementationType.
        /// </summary>
        public InjectorCallSite? InjectorCallSite { get; }
        /// <summary>
        /// Information for cache access related to this service implementation.
        /// </summary>
        public ServiceCacheInfo CacheInfo { get; }
        /// <summary>
        /// Cached result of the call site. 
        /// </summary>
        public object? Value;

        /// <summary>
        /// Whether the produced type should be captured for disposal.
        /// </summary>
        public bool IsTypeDisposable =>
            ImplementationType == null || typeof(IDisposable).IsAssignableFrom(ImplementationType);

        public ServiceCallSite(ServiceCacheInfo cacheInfo, InjectorCallSite? injectorCallSite)
        {
            CacheInfo = cacheInfo;
            InjectorCallSite = injectorCallSite;
        }
    }
}