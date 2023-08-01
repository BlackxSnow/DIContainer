using System;
using DIContainer.Provider;
using IServiceProvider = DIContainer.Provider.IServiceProvider;

namespace DIContainer.CallSite
{
    public class ServiceProviderCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.ServiceProvider;
        public override Type ServiceType => typeof(IServiceProvider);
        public override Type ImplementationType => typeof(ServiceProvider);

    }
}