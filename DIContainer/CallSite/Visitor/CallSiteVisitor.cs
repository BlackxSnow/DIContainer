using System;
using DIContainer.Injector;

namespace DIContainer.CallSite.Visitor
{
    public abstract class CallSiteVisitor<TResult, TContext>
    {
        protected abstract InjectorCallSiteVisitor<TContext> InjectorCallSiteVisitor { get; }

        protected TResult VisitCallSiteCache(ServiceCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }

        protected TResult VisitCallSite(ServiceCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        
        protected TResult VisitRootCache(ServiceCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected TResult VisitScopeCache(ServiceCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected TResult VisitDisposeCache(ServiceCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected TResult VisitNoCache(ServiceCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }

        protected TResult VisitConstructor(ConstructorCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected TResult VisitConstant(ConstantCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected TResult VisitServiceProvider(ServiceProviderCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected TResult VisitEnumerable(EnumerableCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        protected TResult VisitFactory(FactoryCallSite callSite, TContext context)
        {
            throw new NotImplementedException();
        }
        
        protected TResult InjectViaMethod(ServiceCallSite callSite, TContext context, TResult instance)
        {
            throw new NotImplementedException();
        }
        protected TResult InjectViaProperties(ServiceCallSite callSite, TContext context, TResult instance)
        {
            throw new NotImplementedException();
        }
    }
}