using DIContainer.CallSite.Visitor;

namespace DIContainer.Injector.Visitor
{
    internal class InjectorRuntimeResolver : InjectorCallSiteVisitor<RuntimeResolverContext>
    {
        protected override void VisitMethod(InjectorCallSite callSite, RuntimeResolverContext context)
        {
            throw new System.NotImplementedException();
        }

        protected override void VisitSingleProperty(PropertyInjectionPoint property, RuntimeResolverContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}