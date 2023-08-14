using DIContainer.Provider;

namespace DIContainer.Injector.Visitor
{
    /// <summary>
    /// Contains data used during <see cref="InjectorRuntimeResolver"/> operations.
    /// </summary>
    public class InjectorRuntimeResolverContext
    {
        /// <summary>
        /// The scope to resolve dependencies through.
        /// </summary>
        public ServiceProviderScope Scope;
        /// <summary>
        /// The injection target instance.
        /// </summary>
        public object? Instance;

        public InjectorRuntimeResolverContext(ServiceProviderScope scope, object? instance)
        {
            Scope = scope;
            Instance = instance;
        }
    }
}