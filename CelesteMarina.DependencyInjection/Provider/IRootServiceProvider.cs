using System;

namespace CelesteMarina.DependencyInjection.Provider
{
    internal interface IRootServiceProvider : IServiceProvider, IDisposable
    {
        bool IsDisposed { get; }
        event Action Disposed;
        ServiceProviderScope RootScope { get; }
        IServiceProviderScope CreateScope();
    }
}