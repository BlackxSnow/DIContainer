using System;
using CelesteMarina.DependencyInjection.Injector;

namespace CelesteMarina.DependencyInjection.Provider
{
    public interface IServiceProviderScope : IDisposable
    {
        IServiceProvider ServiceProvider { get; }
        IServiceInjector ServiceInjector { get; }
        bool IsDisposed { get; }
    }
}