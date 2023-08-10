using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;

namespace DIContainer.Provider.Engine
{
    internal class RuntimeServiceProviderEngine : ServiceProviderEngineBase
    {
        public override ServiceResolver BuildResolver(ServiceCallSite callSite)
        {
            return scope => RuntimeResolver.Resolve(callSite, scope);
        }

        public override ServiceInjector BuildInjector(InjectorCallSite callSite)
        {
            throw new System.NotImplementedException();
        }

        public RuntimeServiceProviderEngine(ICallSiteRuntimeResolver runtimeResolver) : base(runtimeResolver)
        {
            
        }
    }
}