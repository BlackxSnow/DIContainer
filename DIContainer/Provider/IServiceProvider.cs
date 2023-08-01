using System;

namespace DIContainer.Provider
{
    public interface IServiceProvider
    {
        TService GetService<TService>();
        object GetService(Type type);
    }
}