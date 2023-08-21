namespace CelesteMarina.DependencyInjection.Injector.Visitor
{
    /// <summary>
    /// Provides immediate dependency injection for object instances through <see cref="InjectionAttribute"/> properties and methods.
    /// </summary>
    internal interface IInjectorRuntimeResolver
    {
        void Inject(InjectorCallSite callSite, InjectorRuntimeResolverContext context);
    }
}