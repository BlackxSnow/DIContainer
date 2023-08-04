namespace DIContainer.Injector.Visitor
{
    public interface IInjectorRuntimeResolver
    {
        void Inject(InjectorCallSite callSite, InjectorRuntimeResolverContext context);
    }
}