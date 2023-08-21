using CelesteMarina.DependencyInjection.Provider;

namespace CelesteMarina.DependencyInjection
{
    public delegate object ServiceFactory(IServiceProvider provider);
    
    public delegate object? ServiceResolver(ServiceProviderScope scope);
    
    public delegate object? ServiceInjector(ServiceProviderScope scope, object instance);
}