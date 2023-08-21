using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;

namespace CelesteMarina.DependencyInjection.Provider.Engine
{
    public interface IServiceProviderEngine
    {
        ICallSiteRuntimeResolver RuntimeResolver { get; }
        ServiceResolver BuildResolver(ServiceCallSite callSite);
        ServiceInjector BuildInjector(InjectorCallSite callSite);
    }
}