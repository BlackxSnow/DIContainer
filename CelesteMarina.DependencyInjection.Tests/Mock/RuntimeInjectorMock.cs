using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;

namespace CelesteMarina.DependencyInjection.Tests.Mock
{
    public class RuntimeInjectorMock : IInjectorRuntimeResolver
    {
        public void Inject(InjectorCallSite callSite, InjectorRuntimeResolverContext context)
        {
                
        }
    }
}