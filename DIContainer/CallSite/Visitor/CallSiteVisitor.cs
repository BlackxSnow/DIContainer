using System;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;
using DIContainer.Service;

namespace DIContainer.CallSite.Visitor
{
    public abstract class CallSiteVisitor<TContext, TResult>
    {

        protected virtual TResult VisitCallSiteCache(ServiceCallSite callSite, TContext context)
        {
            return callSite.CacheInfo.Location switch
            {
                CacheLocation.Root => VisitRootCache(callSite, context),
                CacheLocation.Scope => VisitScopeCache(callSite, context),
                CacheLocation.Dispose => VisitDisposeCache(callSite, context),
                CacheLocation.None => VisitNoCache(callSite, context),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected virtual TResult VisitCallSite(ServiceCallSite callSite, TContext context)
        {
            return callSite.Kind switch
            {
                CallSiteKind.Constant => VisitConstant((ConstantCallSite)callSite, context),
                CallSiteKind.Constructor => VisitConstructor((ConstructorCallSite)callSite, context),
                CallSiteKind.Factory => VisitFactory((FactoryCallSite)callSite, context),
                CallSiteKind.Enumerable => VisitEnumerable((EnumerableCallSite)callSite, context),
                CallSiteKind.ServiceProvider => VisitServiceProvider((ServiceProviderCallSite)callSite, context),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        protected virtual TResult VisitRootCache(ServiceCallSite callSite, TContext context)
        {
            return VisitCallSite(callSite, context);
        }
        protected virtual TResult VisitScopeCache(ServiceCallSite callSite, TContext context)
        {
            return VisitCallSite(callSite, context);
        }
        protected virtual TResult VisitDisposeCache(ServiceCallSite callSite, TContext context)
        {
            return VisitCallSite(callSite, context);
        }
        protected virtual TResult VisitNoCache(ServiceCallSite callSite, TContext context)
        {
            return VisitCallSite(callSite, context);
        }

        protected abstract TResult VisitConstructor(ConstructorCallSite callSite, TContext context);
        protected abstract TResult VisitConstant(ConstantCallSite callSite, TContext context);
        protected abstract TResult VisitServiceProvider(ServiceProviderCallSite callSite, TContext context);
        protected abstract TResult VisitEnumerable(EnumerableCallSite callSite, TContext context);
        protected abstract TResult VisitFactory(FactoryCallSite callSite, TContext context);
        
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