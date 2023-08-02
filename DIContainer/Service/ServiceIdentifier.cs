using System;

namespace DIContainer.Service
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
    }
}