using DIContainer.CallSite;
using DIContainer.Injector;

namespace DIContainer.Provider.Engine
{
    public interface IServiceProviderEngine
    {
        ServiceResolver BuildResolver(ServiceCallSite callSite);
        ServiceInjector BuildInjector(InjectorCallSite callSite);
    }
}