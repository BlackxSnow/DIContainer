using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;
using CelesteMarina.DependencyInjection.Provider;

namespace CelesteMarina.DependencyInjection.Tests.Mock
{
    public class RuntimeInjectorMock : IInjectorRuntimeResolver
    {
        public void Inject(InjectorCallSite callSite, InjectorRuntimeResolverContext context)
        {
                
        }

        public void Inject(InjectorCallSite callSite, ServiceProviderScope scope, object? instance)
        {
            
        }
    }
}