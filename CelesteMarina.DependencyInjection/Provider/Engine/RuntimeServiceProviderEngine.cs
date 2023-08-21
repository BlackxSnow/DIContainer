using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;

namespace CelesteMarina.DependencyInjection.Provider.Engine
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