using System;
using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Injector.Visitor;

namespace DIContainer.Injector
{
    internal class InjectorILBuilder : InjectorCallSiteVisitor<ILResolverContext>
    {
        public void Build(InjectorCallSite callSite, ILResolverContext context,
            Func<ServiceCallSite, ILResolverContext, object?> visitCallSite)
        {
            throw new NotImplementedException();
        }

        public ServiceInjector BuildDelegate(InjectorCallSite callSite)
        {
            throw new NotImplementedException();
        }

        protected override void VisitMethod(InjectorCallSite callSite, ILResolverContext context)
        {
            throw new NotImplementedException();
        }

        protected override void VisitSingleProperty(PropertyInjectionPoint property, ILResolverContext context)
        {
            throw new NotImplementedException();
        }
    }
}