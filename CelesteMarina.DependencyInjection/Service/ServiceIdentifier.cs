using System;

namespace CelesteMarina.DependencyInjection.Service
{
    public struct ServiceIdentifier
    {
        public Type ServiceType;

        public ServiceIdentifier(ServiceDescriptor descriptor)
        {
            ServiceType = descriptor.ServiceType;
        }

        public ServiceIdentifier(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public ServiceIdentifier GetGenericTypeDefinition()
        {
            return new ServiceIdentifier(ServiceType.GetGenericTypeDefinition());
        }
    }
}