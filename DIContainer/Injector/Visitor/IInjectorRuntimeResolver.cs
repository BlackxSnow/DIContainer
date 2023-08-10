namespace DIContainer.Injector.Visitor
{
    internal interface IInjectorRuntimeResolver
    {
        void Inject(InjectorCallSite callSite, InjectorRuntimeResolverContext context);
    }
}