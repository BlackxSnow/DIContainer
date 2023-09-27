using System;
using System.Threading;
using System.Threading.Tasks;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using Microsoft.Extensions.Logging;

namespace CelesteMarina.DependencyInjection.Provider.Engine
{
    internal class DynamicServiceProviderEngine : ILServiceProviderEngine
    {
        private readonly ServiceProvider _ServiceProvider;

        public override ServiceResolver BuildResolver(ServiceCallSite callSite)
        {
            var callCount = 0;
            return providerScope =>
            {
                using IDisposable? scope =
                    Logger?.BeginScope("Building resolver for {CallSiteServiceType}", callSite.ServiceType);
                object? resolved = RuntimeResolver.Resolve(callSite, providerScope);
                
                if (Interlocked.Increment(ref callCount) != 1) return resolved;
                
                Logger?.LogDebug("Starting compiled resolver build");
                Task.Run(() => BuildCompiledResolver(callSite));
                return resolved;
            };
        }

        private void BuildCompiledResolver(ServiceCallSite callSite)
        {
            ServiceResolver resolver = ILResolver.Build(callSite);
            _ServiceProvider.ReplaceServiceAccessor(callSite, resolver);
        }

        public override void OnInitialisationComplete(IServiceProvider provider)
        {
            Logger = provider.GetService<ILogger<DynamicServiceProviderEngine>>();
        }

        public DynamicServiceProviderEngine(ICallSiteRuntimeResolver runtimeResolver, ICallSiteILResolver ilResolver,
            ServiceProvider provider, ILogger<DynamicServiceProviderEngine>? logger) 
            : base(runtimeResolver, ilResolver, logger)
        {
            _ServiceProvider = provider;
        }

    }
}