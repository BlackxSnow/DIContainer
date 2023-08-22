using System;
using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.Extensions
{
    public static partial class ServiceCollectionDescriptorExtensions
    {
        private static IServiceCollection Add(IServiceCollection services, ServiceDescriptor descriptor)
        {
            services.Add(descriptor);
            return services;
        }
        
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services)
        {
            return Add(services, ServiceDescriptor.Transient<TService,TImplementation>());
        }

        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType,
            Type implementationType)
        {
            return Add(services, ServiceDescriptor.Transient(serviceType, implementationType));
        }

        public static IServiceCollection AddTransient<TService>(this IServiceCollection services,
            ServiceFactory factory)
        {
            return Add(services, ServiceDescriptor.Transient<TService>(factory));
        }
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType,
            ServiceFactory factory)
        {
            return Add(services, ServiceDescriptor.Transient(serviceType, factory));
        }
        
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services)
        {
            return Add(services, ServiceDescriptor.Scoped<TService,TImplementation>());
        }

        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType,
            Type implementationType)
        {
            return Add(services, ServiceDescriptor.Scoped(serviceType, implementationType));
        }

        public static IServiceCollection AddScoped<TService>(this IServiceCollection services,
            ServiceFactory factory)
        {
            return Add(services, ServiceDescriptor.Scoped<TService>(factory));
        }
        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType,
            ServiceFactory factory)
        {
            return Add(services, ServiceDescriptor.Scoped(serviceType, factory));
        }
        
        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services)
        {
            return Add(services, ServiceDescriptor.Singleton<TService,TImplementation>());
        }

        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType,
            Type implementationType)
        {
            return Add(services, ServiceDescriptor.Singleton(serviceType, implementationType));
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService instance)
        {
            return Add(services, ServiceDescriptor.Singleton<TService>(instance));
        }

        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, object instance)
        {
            return Add(services, ServiceDescriptor.Singleton(serviceType, instance));
        }
        
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services,
            ServiceFactory factory)
        {
            return Add(services, ServiceDescriptor.Singleton<TService>(factory));
        }
        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType,
            ServiceFactory factory)
        {
            return Add(services, ServiceDescriptor.Singleton(serviceType, factory));
        }
    }
}