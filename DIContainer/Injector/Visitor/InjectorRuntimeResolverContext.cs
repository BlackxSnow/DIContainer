using DIContainer.Provider;

namespace DIContainer.Injector.Visitor
{
    public class InjectorRuntimeResolverContext
    {
        public ServiceProviderScope Scope;
        public object? Instance;

        public InjectorRuntimeResolverContext(ServiceProviderScope scope, object? instance)
        {
            Scope = scope;
            Instance = instance;
        }
    }
}