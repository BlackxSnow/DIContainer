using System;

namespace DIContainer.Provider
{
    internal interface IRootServiceProvider : IServiceProvider, IDisposable
    {
        Action Disposed { get; set; }
        ServiceProviderScope RootScope { get; }
        IServiceProviderScope CreateScope();
    }
}