using System;

namespace DIContainer.Injector
{
    public class InjectorCallSiteVisitor<TContext>
    {
        protected void VisitCallSite(InjectorCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected void VisitMethod(InjectorCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected void VisitProperties(InjectorCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected void VisitSingleProperty(PropertyInjectionPoint property, TContext context)
        {
            throw new NotImplementedException();
        }
    }
}