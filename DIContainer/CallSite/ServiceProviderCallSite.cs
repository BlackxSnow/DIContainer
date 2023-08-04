using System;
using DIContainer.Injector;
using DIContainer.Provider;
using DIContainer.Service;
using IServiceProvider = DIContainer.Provider.IServiceProvider;

namespace DIContainer.CallSite
{
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