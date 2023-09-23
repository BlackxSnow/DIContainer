using System;

namespace CelesteMarina.DependencyInjection.Provider
{
    public interface IServiceProviderScope : IDisposable
    {
        IServiceProvider ServiceProvider { get; }
        bool IsDisposed { get; }
    }
}