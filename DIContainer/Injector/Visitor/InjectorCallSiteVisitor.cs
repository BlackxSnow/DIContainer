using System;

namespace DIContainer.Injector.Visitor
{
    public abstract class InjectorCallSiteVisitor<TContext>
    {
        protected void VisitCallSite(InjectorCallSite callSite, TContext context)
        {
            if (callSite.MethodInjectionPoint != null) VisitMethod(callSite, context);
            VisitProperties(callSite, context);
        }

        protected abstract void VisitMethod(InjectorCallSite callSite, TContext context);
        protected void VisitProperties(InjectorCallSite callSite, TContext context)
        {
            if (callSite.PropertyInjectionPoints == null) return;
            foreach (PropertyInjectionPoint propertyInjectionPoint in callSite.PropertyInjectionPoints)
            {
                VisitSingleProperty(propertyInjectionPoint, context);
            }
        }

        protected abstract void VisitSingleProperty(PropertyInjectionPoint property, TContext context);
    }
}