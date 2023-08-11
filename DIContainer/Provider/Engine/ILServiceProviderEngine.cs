using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;

namespace DIContainer.Provider.Engine
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