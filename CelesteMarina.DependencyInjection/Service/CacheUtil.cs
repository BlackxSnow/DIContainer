using System;

namespace CelesteMarina.DependencyInjection.Service
{
    public static class CacheUtil
    {
        public static CacheLocation GetCommonCacheLocation(CacheLocation a, CacheLocation b)
        {
            return (CacheLocation)Math.Max((int)a, (int)b);
        }
    }
}