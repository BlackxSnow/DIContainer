using System;

namespace DIContainer.Provider
{
    public interface IServiceProviderScope : IDisposable
    {
        bool IsDisposed { get; }
    }
}