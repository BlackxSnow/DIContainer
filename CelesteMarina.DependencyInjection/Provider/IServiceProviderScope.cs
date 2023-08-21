using System;

namespace CelesteMarina.DependencyInjection.Provider
{
    public interface IServiceProviderScope : IDisposable
    {
        bool IsDisposed { get; }
    }
}