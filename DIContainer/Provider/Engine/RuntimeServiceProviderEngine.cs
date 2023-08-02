using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;

namespace DIContainer.Provider.Engine
{
    public class RuntimeServiceProviderEngine : IServiceProviderEngine
    {
        public ServiceResolver BuildResolver(ServiceCallSite callSite)
        {
            return scope => CallSiteRuntimeResolver.Instance.Resolve(callSite, scope);
        }

        public ServiceInjector BuildInjector(InjectorCallSite callSite)
        {
            throw new System.NotImplementedException();
        }
    }
}