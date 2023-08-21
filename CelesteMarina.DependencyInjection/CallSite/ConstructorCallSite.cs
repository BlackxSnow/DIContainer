using System;
using System.Reflection;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.CallSite
{
    /// <summary>
    /// Contains resolution data for a service implementation that uses a constructor.
    /// </summary>
    public class ConstructorCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Constructor;
        public override Type ServiceType { get; }
        public override Type? ImplementationType => ConstructorInfo.DeclaringType;

        public ConstructorInfo ConstructorInfo { get; }
        public ServiceCallSite[] ParameterCallSites { get; }

        public ConstructorCallSite(ServiceCacheInfo cacheInfo, InjectorCallSite? injectorCallSite, Type serviceType,
            ConstructorInfo contructorInfo, ServiceCallSite[] parameterCallSites) : base(cacheInfo, injectorCallSite)
        {
            if (!serviceType.IsAssignableFrom(contructorInfo.DeclaringType))
            {
                throw new ArgumentException(string.Format(Exceptions.ImplementationTypeCantConvertToServiceType, ConstructorInfo!.DeclaringType, serviceType));
            }
            ServiceType = serviceType;
            ConstructorInfo = contructorInfo;
            ParameterCallSites = parameterCallSites;
        }
    }
}