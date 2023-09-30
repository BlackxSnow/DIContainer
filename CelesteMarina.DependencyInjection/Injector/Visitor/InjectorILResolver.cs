using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Utility;

namespace CelesteMarina.DependencyInjection.Injector.Visitor
{
    /// <summary>
    /// <inheritdoc cref="IInjectorILResolver"/>
    /// </summary>
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
            if (callSite.IsEmpty()) return;
            VisitCallSite(callSite, context);
        }

        public ServiceInjector BuildDelegate(InjectorCallSite callSite)
        {
            if (callSite.IsEmpty()) return (_,instance) => instance;
            if (_InjectorCache.TryGetValue(callSite.TargetType, out InjectorMethod method)) return method.Lambda;
            
            method = BuildMethod(callSite);
            _InjectorCache.Add(callSite.TargetType, method);

            return method.Lambda;
        }

        /// <summary>
        /// Build an <see cref="InjectorMethod"/> for <paramref name="callSite"/>.
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="IInjectorILResolver.Build"/></param>
        /// <returns>The built <see cref="InjectorMethod"/>.</returns>
        private InjectorMethod BuildMethod(InjectorCallSite callSite)
        {
            var method = new DynamicMethod($"ResolveService",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(object),
                new[] { typeof(IL.RuntimeContext), typeof(ServiceProviderScope), typeof(object) },
                GetType(),
                true);
            
            ILGenerator generator = method.GetILGenerator();
            
            LocalBuilder instance = generator.DeclareLocal(typeof(object));
            
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Stloc, instance);

            var context = new ILInjectorContext(new ILResolverContext(generator), instance);
            VisitCallSite(callSite, context);
            
            generator.Emit(OpCodes.Ldloc, instance);
            generator.Emit(OpCodes.Ret);

            var runtimeContext = new IL.RuntimeContext()
            {
                Constants = context.ResolverContext.Constants?.ToArray(),
                Factories = context.ResolverContext.Factories?.ToArray()
            };

            return new InjectorMethod()
            {
                Context = runtimeContext,
                DynamicMethod = method,
                Lambda = (ServiceInjector)method.CreateDelegate(typeof(ServiceInjector), runtimeContext)
            };
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