using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;
using Microsoft.Extensions.Logging;

namespace CelesteMarina.DependencyInjection.Provider.Engine
{
    internal abstract class ServiceProviderEngineBase : IServiceProviderEngine
    {
        public ICallSiteRuntimeResolver RuntimeResolver { get; }
        protected ILogger Logger;
        public abstract ServiceResolver BuildResolver(ServiceCallSite callSite);
        public abstract ServiceInjector BuildInjector(InjectorCallSite callSite);

        protected ServiceProviderEngineBase(ICallSiteRuntimeResolver runtimeResolver, ILogger<ServiceProviderEngineBase> logger)
        {
            RuntimeResolver = runtimeResolver;
            Logger = logger;
        }
    }
}