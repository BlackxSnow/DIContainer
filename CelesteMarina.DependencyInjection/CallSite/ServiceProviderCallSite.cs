using System;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Service;
using IServiceProvider = CelesteMarina.DependencyInjection.Provider.IServiceProvider;

namespace CelesteMarina.DependencyInjection.CallSite
{
    /// <summary>
    /// Contains resolution data for IServiceProvider requests.
    /// </summary>
    public class ServiceProviderCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.ServiceProvider;
        public override Type ServiceType => typeof(Provider.IServiceProvider);
        public override Type ImplementationType => typeof(ServiceProvider);

        public ServiceProviderCallSite(ServiceCacheInfo cacheInfo, InjectorCallSite injectorCallSite) : base(cacheInfo,
            injectorCallSite)
        {

        }
    }
}