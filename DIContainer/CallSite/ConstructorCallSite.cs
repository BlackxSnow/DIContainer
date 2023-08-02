using System;
using System.Reflection;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    public class ConstructorCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Constructor;
        public override Type ServiceType { get; }
        public override Type? ImplementationType => ConstructorInfo.DeclaringType;

        public ConstructorInfo ConstructorInfo { get; }
        public ServiceCallSite[] ParameterCallSites { get; }

        public ConstructorCallSite(ServiceCacheInfo cacheInfo, Type serviceType, ConstructorInfo contructorInfo, ServiceCallSite[] parameterCallSites) : base(cacheInfo)
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