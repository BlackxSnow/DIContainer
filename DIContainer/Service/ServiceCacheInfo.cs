using System;

namespace DIContainer.Service
{
    public struct ServiceCacheInfo
    {
        public ServiceCacheKey CacheKey { get; }
        public CacheLocation Location { get; }

        public static ServiceCacheInfo None(Type serviceType)
        {
            return new ServiceCacheInfo(CacheLocation.None, new ServiceCacheKey(new ServiceIdentifier(serviceType)));
        }
        
        public ServiceCacheInfo(ServiceLifetime lifetime, ServiceIdentifier identifier)
        {
            CacheKey = new ServiceCacheKey(identifier);
            Location = lifetime switch
            {
                ServiceLifetime.Singleton => CacheLocation.Root,
                ServiceLifetime.Scoped => CacheLocation.Scope,
                ServiceLifetime.Transient => CacheLocation.Dispose,
                _ => CacheLocation.None
            };
        }

        public ServiceCacheInfo(CacheLocation location, ServiceCacheKey key)
        {
            Location = location;
            CacheKey = key;
        }
    }
}