using System;
using DIContainer.Service;

namespace DIContainer.CallSite
{
    /// <summary>
    /// Contains resolution data for a constant-value implementation of a service.
    /// </summary>
    public class ConstantCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind => CallSiteKind.Constant;
        public override Type ServiceType { get; }
        public override Type? ImplementationType => Value?.GetType(); 

        public ConstantCallSite(Type serviceType, object instance) : base(ServiceCacheInfo.None(serviceType), null)
        {
            if (!serviceType.IsInstanceOfType(instance))
            {
                throw new ArgumentException(string.Format(Exceptions.ConstantCantConvertToServiceType, serviceType));
            }
            Value = instance;
            ServiceType = serviceType;
        }
    }
}