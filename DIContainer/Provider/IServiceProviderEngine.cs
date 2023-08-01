using DIContainer.CallSite;
using DIContainer.Injector;

namespace DIContainer.Provider
{
    public interface IServiceProviderEngine
    {
        ServiceResolver BuildResolver(ServiceCallSite callSite);
        ServiceInjector BuildInjector(InjectorCallSite callSite);
    }
}