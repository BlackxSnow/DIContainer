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
    public class ServiceInjectorCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.ServiceInjector;
        public override Type ServiceType => typeof(IServiceInjector);
        public override Type ImplementationType => typeof(IServiceInjector);

        public ServiceInjectorCallSite() : base(ServiceCacheInfo.None(typeof(IServiceInjector)), null)
        {

        }
    }
}