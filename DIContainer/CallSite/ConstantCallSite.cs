using System;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    public class ConstantCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Constant;
        public override Type ServiceType { get; }
        public override Type? ImplementationType => Value?.GetType(); 

        public ConstantCallSite(Type serviceType, object instance) : base(ServiceCacheInfo.None(serviceType))
        {
            if (!serviceType.IsInstanceOfType(instance))
            {
                throw new ArgumentException(Exceptions.ConstantCantConvertToServiceType);
            }
            Value = instance;
            ServiceType = serviceType;
        }
    }
}