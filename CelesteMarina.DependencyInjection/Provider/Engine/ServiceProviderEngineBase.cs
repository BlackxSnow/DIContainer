using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;

namespace CelesteMarina.DependencyInjection.Provider.Engine
{
    internal abstract class ServiceProviderEngineBase : IServiceProviderEngine
    {
        public ICallSiteRuntimeResolver RuntimeResolver { get; }
        public abstract ServiceResolver BuildResolver(ServiceCallSite callSite);
        public abstract ServiceInjector BuildInjector(InjectorCallSite callSite);

        protected ServiceProviderEngineBase(ICallSiteRuntimeResolver runtimeResolver)
        {
            RuntimeResolver = runtimeResolver;
        }
    }
}