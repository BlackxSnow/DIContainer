using System;
using System.Collections.Generic;
using DIContainer.Service;

namespace DIContainer.Provider.Temporary
{
    public interface ITemporaryServiceContainer : IDisposable
    {
        bool IsDisposed { get; }
        event Action<ITemporaryServiceContainer> Disposed;
        ITemporaryServiceCollection Services { get; }
    }
}