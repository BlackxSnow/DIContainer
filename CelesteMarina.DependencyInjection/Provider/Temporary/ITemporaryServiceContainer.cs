using System;
using System.Collections.Generic;
using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.Provider.Temporary
{
    public interface ITemporaryServiceContainer : IDisposable
    {
        bool IsDisposed { get; }
        event Action<ITemporaryServiceContainer> Disposed;
        ITemporaryServiceCollection Services { get; }
    }
}