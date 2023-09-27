using System;

namespace CelesteMarina.DependencyInjection.Provider
{
    internal interface IRootServiceProvider : IServiceProvider, IDisposable
    {
        bool IsDisposed { get; }
        event Action Disposed;
        ServiceProviderScope RootScope { get; }
        object? GetService(Type type, ServiceProviderScope scope);
        IServiceProviderScope CreateScope();
    }
}