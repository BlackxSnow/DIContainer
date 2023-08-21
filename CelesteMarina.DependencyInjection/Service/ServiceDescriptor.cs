using System;
using System.Collections.Generic;
using CelesteMarina.DependencyInjection.Provider.Temporary;

namespace CelesteMarina.DependencyInjection.Service
{
    public class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public ServiceLifetime Lifetime { get; }
        public ITemporaryServiceContainer? Container { get; }
        public Type? ImplementationType { get; }
        public object? ImplementationInstance { get; }
        public ServiceFactory? ImplementationFactory { get; }
        public IReadOnlyCollection<Type>? ImplementationNotableTypes { get; }

        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        public ServiceDescriptor(Type serviceType, object instance)
        {
            ServiceType = serviceType;
            ImplementationInstance = instance;
        }

        public ServiceDescriptor(Type serviceType, ServiceFactory factory, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationFactory = factory;
            Lifetime = lifetime;
        }
    }
}