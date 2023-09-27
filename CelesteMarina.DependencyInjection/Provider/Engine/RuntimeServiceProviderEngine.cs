using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;
using Microsoft.Extensions.Logging;

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

        public override void OnInitialisationComplete(IServiceProvider provider)
        {
            Logger = provider.GetService<ILogger<RuntimeServiceProviderEngine>>();
        }

        public RuntimeServiceProviderEngine(ICallSiteRuntimeResolver runtimeResolver,
            ILogger<RuntimeServiceProviderEngine>? logger) : base(runtimeResolver, logger)
        {
            
        }
    }
}