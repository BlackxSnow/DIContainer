using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;

namespace DIContainer.Provider.Engine
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