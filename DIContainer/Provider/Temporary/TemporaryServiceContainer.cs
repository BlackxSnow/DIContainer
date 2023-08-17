using System;
using System.Collections.Generic;
using DIContainer.Service;

namespace DIContainer.Provider.Temporary
{
    public class TemporaryServiceContainer : ITemporaryServiceContainer
    {
        public bool IsDisposed { get; private set; }
        public event Action<ITemporaryServiceContainer>? Disposed;
        public ITemporaryServiceCollection Services { get; }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Disposed?.Invoke(this);
        }

        public TemporaryServiceContainer(params ServiceDescriptor[] services)
        {
            Services = new TemporaryServiceCollection(services);
        }
    }
}