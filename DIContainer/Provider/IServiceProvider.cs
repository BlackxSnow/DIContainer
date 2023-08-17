using System;

namespace DIContainer.Provider
{
    /// <summary>
    /// Provides retrieval of previously registered services by type.
    /// </summary>
    public interface IServiceProvider
    {
        TService? GetService<TService>();
        object? GetService(Type type);
    }
}