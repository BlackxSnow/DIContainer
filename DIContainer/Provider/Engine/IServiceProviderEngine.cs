using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;

namespace DIContainer.Provider.Engine
{
    public interface IServiceProviderEngine
    {
        ICallSiteRuntimeResolver RuntimeResolver { get; }
        ServiceResolver BuildResolver(ServiceCallSite callSite);
        ServiceInjector BuildInjector(InjectorCallSite callSite);
    }
}