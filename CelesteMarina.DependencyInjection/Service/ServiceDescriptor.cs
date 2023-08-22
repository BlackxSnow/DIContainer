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

        public static ServiceDescriptor Transient<TService, TImplementation>() =>
            Transient(typeof(TService), typeof(TImplementation));
        public static ServiceDescriptor Transient(Type serviceType, Type implementationType)
        {
            return new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
        }

        public static ServiceDescriptor Transient<TService>(ServiceFactory factory) =>
            Transient(typeof(TService), factory);
        public static ServiceDescriptor Transient(Type serviceType, ServiceFactory factory)
        {
            return new ServiceDescriptor(serviceType, factory, ServiceLifetime.Transient);
        }
        
        public static ServiceDescriptor Scoped<TService, TImplementation>() =>
            Scoped(typeof(TService), typeof(TImplementation));
        public static ServiceDescriptor Scoped(Type serviceType, Type implementationType)
        {
            return new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Scoped);
        }

        public static ServiceDescriptor Scoped<TService>(ServiceFactory factory) =>
            Scoped(typeof(TService), factory);
        public static ServiceDescriptor Scoped(Type serviceType, ServiceFactory factory)
        {
            return new ServiceDescriptor(serviceType, factory, ServiceLifetime.Scoped);
        }
        
        public static ServiceDescriptor Singleton<TService, TImplementation>() =>
            Singleton(typeof(TService), typeof(TImplementation));
        public static ServiceDescriptor Singleton(Type serviceType, Type implementationType)
        {
            return new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton);
        }

        public static ServiceDescriptor Singleton<TService>(object instance) => Singleton(typeof(TService), instance);
        public static ServiceDescriptor Singleton(Type serviceType, object instance)
        {
            return new ServiceDescriptor(serviceType, instance);
        }
        
        public static ServiceDescriptor Singleton<TService>(ServiceFactory factory) =>
            Singleton(typeof(TService), factory);
        public static ServiceDescriptor Singleton(Type serviceType, ServiceFactory factory)
        {
            return new ServiceDescriptor(serviceType, factory, ServiceLifetime.Singleton);
        }
    }
}