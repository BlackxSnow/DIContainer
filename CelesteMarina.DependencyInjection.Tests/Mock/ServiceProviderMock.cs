using System;
using CelesteMarina.DependencyInjection.Provider;

namespace CelesteMarina.DependencyInjection.Tests.Mock
{
    public class ServiceProviderMock : IRootServiceProvider
    {
        public TService GetService<TService>() => throw new NotSupportedException();

        public object GetService(Type type) => throw new NotSupportedException();

        public bool IsDisposed { get; private set; }
        public event Action Disposed;
        public ServiceProviderScope RootScope { get; }
        public IServiceProviderScope CreateScope()
        {
            return new ServiceProviderScope(this, false);
        }

        public ServiceProviderMock()
        {
            RootScope = new ServiceProviderScope(this, true);
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            RootScope.Dispose();
            Disposed?.Invoke();
        }
    }
}