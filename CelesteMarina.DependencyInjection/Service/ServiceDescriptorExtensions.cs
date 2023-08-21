using System;

namespace CelesteMarina.DependencyInjection.Service
{
    public static class ServiceDescriptorExtensions
    {
        public static bool HasInstance(this ServiceDescriptor descriptor) => descriptor.GetInstance() != null;
        public static bool HasFactory(this ServiceDescriptor descriptor) => descriptor.GetFactory() != null;
        public static bool HasImplementationType(this ServiceDescriptor descriptor) =>
            descriptor.GetImplementationType() != null;
        
        public static object? GetInstance(this ServiceDescriptor descriptor) => descriptor.ImplementationInstance;
        public static ServiceFactory? GetFactory(this ServiceDescriptor descriptor) => descriptor.ImplementationFactory;
        public static Type? GetImplementationType(this ServiceDescriptor descriptor) => descriptor.ImplementationType;
    }
}