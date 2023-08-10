using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;
using DIContainer.Utility;

namespace DIContainer.Injector.Visitor
{
    internal class InjectorILResolver : InjectorCallSiteVisitor<ILInjectorContext>, IInjectorILResolver
    {

        private struct InjectorMethod
        {
            public ServiceInjector Lambda;
            public IL.RuntimeContext Context;
            public DynamicMethod DynamicMethod;
        }
        
        private ICallSiteILResolver _CallSiteResolver;

        private Dictionary<Type, InjectorMethod> _InjectorCache;
        
        public void Build(InjectorCallSite callSite, ILInjectorContext context)
        {
            VisitCallSite(callSite, context);
        }

        public ServiceInjector BuildDelegate(InjectorCallSite callSite)
        {
            if (!_InjectorCache.TryGetValue(callSite.TargetType, out InjectorMethod method))
            {
                throw new NotImplementedException();
            }

            return method.Lambda;
        }

        protected override void VisitMethod(InjectorCallSite callSite, ILInjectorContext context)
        {
            context.Generator.Emit(OpCodes.Ldloc, context.Instance);
            ServiceCallSite[] paramCallSites = callSite.MethodInjectionPoint!.ParameterCallSites;
            
            foreach (ServiceCallSite paramCallSite in paramCallSites)
            {
                _CallSiteResolver.BuildInline(paramCallSite, context.ResolverContext);
                if (paramCallSite.ServiceType.IsValueType)
                {
                    context.Generator.Emit(OpCodes.Unbox_Any, paramCallSite.ServiceType);
                }
            }
            
            context.Generator.Emit(OpCodes.Callvirt, callSite.MethodInjectionPoint.Method);
        }

        protected override void VisitSingleProperty(PropertyInjectionPoint property, ILInjectorContext context)
        {
            context.Generator.Emit(OpCodes.Ldloc, context.Instance);
            _CallSiteResolver.BuildInline(property.CallSite, context.ResolverContext);
            context.Generator.Emit(OpCodes.Callvirt, property.Property.SetMethod);
        }

        public InjectorILResolver(ICallSiteILResolver callSiteResolver)
        {
            _CallSiteResolver = callSiteResolver;
            _InjectorCache = new Dictionary<Type, InjectorMethod>();
        }
    }
}