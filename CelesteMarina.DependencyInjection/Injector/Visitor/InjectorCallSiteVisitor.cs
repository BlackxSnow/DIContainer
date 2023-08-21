using System;
using CelesteMarina.DependencyInjection.CallSite.Visitor;

namespace CelesteMarina.DependencyInjection.Injector.Visitor
{
    /// <summary>
    /// Traverses <see cref="InjectorCallSite">InjectorCallSites</see>
    /// </summary>
    /// <typeparam name="TContext"><inheritdoc cref="CallSiteVisitor{TContext,TResult}"/></typeparam>
    public abstract class InjectorCallSiteVisitor<TContext>
    {
        protected void VisitCallSite(InjectorCallSite callSite, TContext context)
        {
            if (callSite.MethodInjectionPoint != null) VisitMethod(callSite, context);
            VisitProperties(callSite, context);
        }

        /// <summary>
        /// Visit the <paramref name="callSite">callSite's</paramref> injection method. 
        /// </summary>
        /// <param name="callSite">The call site for the type to be injected into.</param>
        /// <param name="context">The object passed between methods as context.</param>
        protected abstract void VisitMethod(InjectorCallSite callSite, TContext context);
        /// <summary>
        /// Visit the <paramref name="callSite">callSite's</paramref> injection properties. 
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="VisitMethod"/></param>
        /// <param name="context"><inheritdoc cref="VisitMethod"/></param>
        protected void VisitProperties(InjectorCallSite callSite, TContext context)
        {
            if (callSite.PropertyInjectionPoints == null) return;
            foreach (PropertyInjectionPoint propertyInjectionPoint in callSite.PropertyInjectionPoints)
            {
                VisitSingleProperty(propertyInjectionPoint, context);
            }
        }

        /// <summary>
        /// Visit a single injection property.
        /// </summary>
        /// <param name="property">The injection property to visit.</param>
        /// <param name="context"><inheritdoc cref="VisitMethod"/></param>
        protected abstract void VisitSingleProperty(PropertyInjectionPoint property, TContext context);
    }
}