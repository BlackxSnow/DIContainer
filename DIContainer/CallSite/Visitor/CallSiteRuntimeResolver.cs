using System.Reflection;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;
using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    internal class CallSiteRuntimeResolver : CallSiteVisitor<RuntimeResolverContext, object?>
    {
        public static CallSiteRuntimeResolver Instance { get; } = new();
        protected override InjectorCallSiteVisitor<RuntimeResolverContext> InjectorCallSiteVisitor { get; }

        public object? Resolve(ServiceCallSite callSite, ServiceProviderScope scope)
        {
            if (scope.IsRootScope && callSite.Value != null) return callSite.Value;
            return VisitCallSite(callSite, new RuntimeResolverContext { Scope = scope });
        }
        
        protected override object? VisitConstructor(ConstructorCallSite callSite, RuntimeResolverContext context)
        {
            var arguments = new object?[callSite.ParameterCallSites.Length];
            for (var i = 0; i < arguments.Length; i++)
            {
                ServiceCallSite parameterCallSite = callSite.ParameterCallSites[i];
                arguments[i] = VisitCallSiteCache(parameterCallSite, context);
            }

            return callSite.ConstructorInfo.Invoke(arguments);
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
            throw new System.NotImplementedException();
        }

        protected override object? VisitFactory(FactoryCallSite callSite, RuntimeResolverContext context)
        {
            return callSite.Factory(context.Scope);
        }
    }
}