using System;
using System.Linq;
using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.Extensions
{
    public static partial class ServiceCollectionDescriptorExtensions
    {
        public static IServiceCollection AddOnce(this IServiceCollection services, ServiceDescriptor descriptor)
        {
            return services.Contains(descriptor) ? services : Add(services, descriptor);
        }

        public static IServiceCollection AddOnceTransient<TService>(this IServiceCollection services)
        {
            return services.AddOnce(ServiceDescriptor.Transient<TService, TService>());
        }
        public static IServiceCollection AddOnceTransient(this IServiceCollection services, Type service)
        {
            return services.AddOnce(ServiceDescriptor.Transient(service, service));
        }

        public static IServiceCollection AddOnceTransient<TService, TImplementation>(this IServiceCollection services)
        {
            return services.AddOnce(ServiceDescriptor.Transient<TService, TImplementation>());
        }
        public static IServiceCollection AddOnceTransient(this IServiceCollection services, Type service,
            Type implementationType)
        {
            return services.AddOnce(ServiceDescriptor.Transient(service, implementationType));
        }

        public static IServiceCollection AddOnceTransient<TService>(this IServiceCollection services,
            ServiceFactory factory)
        {
            return services.AddOnce(ServiceDescriptor.Transient<TService>(factory));
        }
        public static IServiceCollection AddOnceTransient(this IServiceCollection services, Type service,
            ServiceFactory factory)
        {
            return services.AddOnce(ServiceDescriptor.Transient(service, factory));
        }
        
        public static IServiceCollection AddOnceScoped<TService>(this IServiceCollection services)
        {
            return services.AddOnce(ServiceDescriptor.Scoped<TService, TService>());
        }
        public static IServiceCollection AddOnceScoped(this IServiceCollection services, Type service)
        {
            return services.AddOnce(ServiceDescriptor.Scoped(service, service));
        }

        public static IServiceCollection AddOnceScoped<TService, TImplementation>(this IServiceCollection services)
        {
            return services.AddOnce(ServiceDescriptor.Scoped<TService, TImplementation>());
        }
        public static IServiceCollection AddOnceScoped(this IServiceCollection services, Type service,
            Type implementationType)
        {
            return services.AddOnce(ServiceDescriptor.Scoped(service, implementationType));
        }

        public static IServiceCollection AddOnceScoped<TService>(this IServiceCollection services,
            ServiceFactory factory)
        {
            return services.AddOnce(ServiceDescriptor.Scoped<TService>(factory));
        }
        public static IServiceCollection AddOnceScoped(this IServiceCollection services, Type service,
            ServiceFactory factory)
        {
            return services.AddOnce(ServiceDescriptor.Scoped(service, factory));
        }
        
        public static IServiceCollection AddOnceSingleton<TService>(this IServiceCollection services)
        {
            return services.AddOnce(ServiceDescriptor.Singleton<TService, TService>());
        }
        public static IServiceCollection AddOnceSingleton(this IServiceCollection services, Type service)
        {
            return services.AddOnce(ServiceDescriptor.Singleton(service, service));
        }

        public static IServiceCollection AddOnceSingleton<TService, TImplementation>(this IServiceCollection services)
        {
            return services.AddOnce(ServiceDescriptor.Singleton<TService, TImplementation>());
        }
        public static IServiceCollection AddOnceSingleton(this IServiceCollection services, Type service,
            Type implementationType)
        {
            return services.AddOnce(ServiceDescriptor.Singleton(service, implementationType));
        }

        public static IServiceCollection AddOnceSingleton<TService>(this IServiceCollection services, TService instance)
        {
            return services.AddOnce(ServiceDescriptor.Singleton<TService>(instance));
        }
        public static IServiceCollection AddOnceSingleton(this IServiceCollection services, Type service,
            object instance)
        {
            return services.AddOnce(ServiceDescriptor.Singleton(service, instance));
        }

        public static IServiceCollection AddOnceSingleton<TService>(this IServiceCollection services,
            ServiceFactory factory)
        {
            return services.AddOnce(ServiceDescriptor.Singleton<TService>(factory));
        }
        public static IServiceCollection AddOnceSingleton(this IServiceCollection services, Type service,
            ServiceFactory factory)
        {
            return services.AddOnce(ServiceDescriptor.Singleton(service, factory));
        }

        public static IServiceCollection AddOnceEnumerable(this IServiceCollection services,
            ServiceDescriptor descriptor)
        {
            Type? implementation = descriptor.GetImplementationType();

            return services.Any(s =>
                s.ServiceType == descriptor.ServiceType && s.GetImplementationType() == implementation) 
                ? services : Add(services, descriptor);
        }
    }
}