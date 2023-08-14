using System;
using DIContainer.Injector;
using DIContainer.Provider;
using DIContainer.Service;
using IServiceProvider = DIContainer.Provider.IServiceProvider;

namespace DIContainer.CallSite
{
    /// <summary>
    /// Contains resolution data for IServiceProvider requests.
    /// </summary>
    public class ServiceProviderCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.ServiceProvider;
        public override Type ServiceType => typeof(IServiceProvider);
        public override Type ImplementationType => typeof(ServiceProvider);

        public ServiceProviderCallSite(ServiceCacheInfo cacheInfo, InjectorCallSite injectorCallSite) : base(cacheInfo,
            injectorCallSite)
        {

        }
    }
}