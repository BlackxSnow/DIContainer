using System;
using CelesteMarina.DependencyInjection.Injector;

namespace CelesteMarina.DependencyInjection.Provider
{
    internal interface IRootServiceProvider : IServiceProvider, IServiceInjector, IDisposable
    {
        bool IsDisposed { get; }
        event Action Disposed;
        ServiceProviderScope RootScope { get; }
        object? GetService(Type type, ServiceProviderScope scope);
        void InjectServices(object instance, ServiceProviderScope scope);
        IServiceProviderScope CreateScope();
    }
}