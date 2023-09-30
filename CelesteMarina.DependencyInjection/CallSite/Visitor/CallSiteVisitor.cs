using System;
using CelesteMarina.DependencyInjection.Service;
using CelesteMarina.DependencyInjection.Injector;
using CelesteMarina.DependencyInjection.Injector.Visitor;

namespace CelesteMarina.DependencyInjection.CallSite.Visitor
{
    /// <summary>
    /// Recursively traverses <see cref="ServiceCallSite">ServiceCallSites</see>.
    /// </summary>
    /// <typeparam name="TContext">The type of object passed between methods as context.</typeparam>
    /// <typeparam name="TResult">The type returned by member methods.</typeparam>
    public abstract class CallSiteVisitor<TContext, TResult>
    {
        /// <summary>
        /// Visit <paramref name="callSite"/>, accessing the associated cache (if any) when appropriate.
        /// </summary>
        /// <param name="callSite">The call site representing the service implementation to resolve.</param>
        /// <param name="context">The object passed between methods as context.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on invalid call site <see cref="CacheLocation"/>.</exception>
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

        /// <summary>
        /// Visit <paramref name="callSite"/> directly, ignoring the cache.
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="VisitCallSiteCache"/></param>
        /// <param name="context"><inheritdoc cref="VisitCallSiteCache"/></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on invalid <see cref="CallSiteKind"/>.</exception>
        protected virtual TResult VisitCallSite(ServiceCallSite callSite, TContext context)
        {
            return callSite.Kind switch
            {
                CallSiteKind.Constant => VisitConstant((ConstantCallSite)callSite, context),
                CallSiteKind.Constructor => VisitConstructor((ConstructorCallSite)callSite, context),
                CallSiteKind.Factory => VisitFactory((FactoryCallSite)callSite, context),
                CallSiteKind.Enumerable => VisitEnumerable((EnumerableCallSite)callSite, context),
                CallSiteKind.ServiceProvider => VisitServiceProvider((ServiceProviderCallSite)callSite, context),
                CallSiteKind.ServiceInjector => VisitServiceInjector((ServiceInjectorCallSite)callSite, context),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        /// <summary>
        /// Visit the root cache for <see cref="ServiceLifetime.Singleton"/> services.
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="VisitCallSiteCache"/></param>
        /// <param name="context"><inheritdoc cref="VisitCallSiteCache"/></param>
        protected virtual TResult VisitRootCache(ServiceCallSite callSite, TContext context)
        {
            return VisitCallSite(callSite, context);
        }
        
        /// <summary>
        /// Visit the scope cache for <see cref="ServiceLifetime.Scoped"/> services.
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="VisitCallSiteCache"/></param>
        /// <param name="context"><inheritdoc cref="VisitCallSiteCache"/></param>
        protected virtual TResult VisitScopeCache(ServiceCallSite callSite, TContext context)
        {
            return VisitCallSite(callSite, context);
        }
        
        /// <summary>
        /// Visit the disposables cache for <see cref="ServiceLifetime.Transient"/> services.
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="VisitCallSiteCache"/></param>
        /// <param name="context"><inheritdoc cref="VisitCallSiteCache"/></param>
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
        protected abstract TResult VisitServiceInjector(ServiceInjectorCallSite callSite, TContext context);
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