using System.Reflection;
using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;

namespace DIContainer.Injector.Visitor
{
    /// <summary>
    /// <inheritdoc cref="IInjectorRuntimeResolver"/>
    /// </summary>
    internal class InjectorRuntimeResolver : InjectorCallSiteVisitor<InjectorRuntimeResolverContext>, IInjectorRuntimeResolver
    {

        public ICallSiteRuntimeResolver ServiceResolver { get; }
        
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
                parameterValues[i] = ServiceResolver.Resolve(parameterCallSites[i], context.Scope);
            }

            callSite.MethodInjectionPoint.Method.Invoke(context.Instance, parameterValues);
        }

        protected override void VisitSingleProperty(PropertyInjectionPoint property, InjectorRuntimeResolverContext context)
        {
            MethodInfo setter = property.Property.GetSetMethod(true);
            object? value = ServiceResolver.Resolve(property.CallSite, context.Scope);
            setter.Invoke(context.Instance, new []{value});
        }

        public InjectorRuntimeResolver(ICallSiteRuntimeResolver serviceResolver)
        {
            ServiceResolver = serviceResolver;
        }
    }
}