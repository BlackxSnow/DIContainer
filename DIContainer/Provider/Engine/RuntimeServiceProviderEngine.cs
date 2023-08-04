using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;

namespace DIContainer.Provider.Engine
{
    internal class RuntimeServiceProviderEngine : IServiceProviderEngine
    {
        public ICallSiteRuntimeResolver RuntimeResolver { get; }
        public ServiceResolver BuildResolver(ServiceCallSite callSite)
        {
            return scope => RuntimeResolver.Resolve(callSite, scope);
        }

        public ServiceInjector BuildInjector(InjectorCallSite callSite)
        {
            throw new System.NotImplementedException();
        }

        public RuntimeServiceProviderEngine()
        {
            RuntimeResolver = new CallSiteRuntimeResolver(res => new InjectorRuntimeResolver(res));
        }
    }
}