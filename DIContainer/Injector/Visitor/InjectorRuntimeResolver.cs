using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;

namespace DIContainer.Injector.Visitor
{
    internal class InjectorRuntimeResolver : InjectorCallSiteVisitor<InjectorRuntimeResolverContext>
    {
        public static InjectorRuntimeResolver Instance { get; } = new();

        public void Inject(InjectorCallSite callSite, InjectorRuntimeResolverContext context)
        {
            VisitCallSite(callSite, context);
        }
        
        protected override void VisitMethod(InjectorCallSite callSite, InjectorRuntimeResolverContext context)
        {
            if (callSite.MethodInjectionPoint == null) return;
            ServiceCallSite[] parameterCallSites = callSite.MethodInjectionPoint.ParameterCallSites;
            var parameterValues = new object?[parameterCallSites.Length];

            for (var i = 0; i < parameterCallSites.Length; i++)
            {
                parameterValues[i] = CallSiteRuntimeResolver.Instance.Resolve(parameterCallSites[i], context.Scope);
            }

            callSite.MethodInjectionPoint.Method.Invoke(context.Instance, parameterValues);
        }

        protected override void VisitSingleProperty(PropertyInjectionPoint property, InjectorRuntimeResolverContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}