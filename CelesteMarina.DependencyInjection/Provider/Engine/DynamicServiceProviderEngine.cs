using System.Threading;
using System.Threading.Tasks;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using Microsoft.Extensions.Logging;

namespace CelesteMarina.DependencyInjection.Provider.Engine
{
    internal class DynamicServiceProviderEngine : ILServiceProviderEngine
    {
        private ServiceProvider _ServiceProvider;

        public override ServiceResolver BuildResolver(ServiceCallSite callSite)
        {
            var callCount = 0;
            return scope =>
            {
                object? resolved = RuntimeResolver.Resolve(callSite, scope);
                if (Interlocked.Increment(ref callCount) == 1) Task.Run(() => BuildCompiledResolver(callSite));
                return resolved;
            };
        }

        private void BuildCompiledResolver(ServiceCallSite callSite)
        {
            ServiceResolver resolver = ILResolver.Build(callSite);
            _ServiceProvider.ReplaceServiceAccessor(callSite, resolver);
        }
        
        public DynamicServiceProviderEngine(ICallSiteRuntimeResolver runtimeResolver, ICallSiteILResolver ilResolver,
            ServiceProvider provider, ILogger<DynamicServiceProviderEngine> logger) 
            : base(runtimeResolver, ilResolver, logger)
        {
            _ServiceProvider = provider;
        }

    }
}