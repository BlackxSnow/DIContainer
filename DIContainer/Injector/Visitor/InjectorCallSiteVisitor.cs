using System;

namespace DIContainer.Injector.Visitor
{
    public abstract class InjectorCallSiteVisitor<TContext>
    {
        protected void VisitCallSite(InjectorCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }

        protected abstract void VisitMethod(InjectorCallSite callSite, TContext context);
        protected void VisitProperties(InjectorCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }

        protected abstract void VisitSingleProperty(PropertyInjectionPoint property, TContext context);
    }
}