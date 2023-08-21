using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;

namespace CelesteMarina.DependencyInjection.Provider.Engine
{
    internal class ILServiceProviderEngine : ServiceProviderEngineBase
    {
        public ICallSiteILResolver ILResolver { get; }
        public override ServiceResolver BuildResolver(ServiceCallSite callSite)
        {
            return ILResolver.Build(callSite);
        }

        public override ServiceInjector BuildInjector(InjectorCallSite callSite)
        {
            return ILResolver.ILInjector.BuildDelegate(callSite);
        }

        public ILServiceProviderEngine(ICallSiteRuntimeResolver runtimeResolver, ICallSiteILResolver ilResolver) : base(runtimeResolver)
        {
            ILResolver = ilResolver;
        }
    }
}