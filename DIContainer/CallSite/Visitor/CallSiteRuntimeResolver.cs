using System;
using System.Collections.Generic;
using System.Reflection;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;
using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    internal class CallSiteRuntimeResolver : CallSiteVisitor<RuntimeResolverContext, object?>, ICallSiteRuntimeResolver
    {
        public IInjectorRuntimeResolver InjectorResolver { get; }

        public object? Resolve(ServiceCallSite callSite, ServiceProviderScope scope)
        {
            if (scope.IsRootScope && callSite.Value != null) return callSite.Value;
            return VisitCallSiteCache(callSite, new RuntimeResolverContext { Scope = scope });
        }
        
        protected override object? VisitConstructor(ConstructorCallSite callSite, RuntimeResolverContext context)
        {
            var arguments = new object?[callSite.ParameterCallSites.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                ServiceCallSite parameterCallSite = callSite.ParameterCallSites[i];
                arguments[i] = VisitCallSiteCache(parameterCallSite, context);
            }

            object? result = callSite.ConstructorInfo.Invoke(arguments);
            InjectorResolver.Inject(callSite.InjectorCallSite,
                new InjectorRuntimeResolverContext(context.Scope, result));
            return result;
        }

        protected override object? VisitConstant(ConstantCallSite callSite, RuntimeResolverContext context)
        {
            return callSite.Value;
        }

        protected override object? VisitServiceProvider(ServiceProviderCallSite callSite, RuntimeResolverContext context)
        {
            return context.Scope;
        }

        protected override object? VisitEnumerable(EnumerableCallSite callSite, RuntimeResolverContext context)
        {
            var results = Array.CreateInstance(callSite.SingleServiceType, callSite.CallSites.Length);
            for (var i = 0; i < callSite.CallSites.Length; i++)
            {
                object? value = VisitCallSite(callSite.CallSites[i], context);
                results.SetValue(value, i);
            }

            return results;
        }

        protected override object? VisitFactory(FactoryCallSite callSite, RuntimeResolverContext context)
        {
            return callSite.Factory(context.Scope);
        }

        protected override object? VisitDisposeCache(ServiceCallSite callSite, RuntimeResolverContext context)
        {
            return context.Scope.CaptureIfDisposable(VisitCallSite(callSite, context));
        }

        public CallSiteRuntimeResolver(Func<CallSiteRuntimeResolver, IInjectorRuntimeResolver> injectorResolverBuilder)
        {
            InjectorResolver = injectorResolverBuilder(this);
        }
    }
}