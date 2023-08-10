using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DIContainer.Injector;
using DIContainer.Provider;
using DIContainer.Service;
using Microsoft.Extensions.Logging;

namespace DIContainer.CallSite.Visitor
{
    internal class CallSiteILResolver : CallSiteVisitor<ILResolverContext, object?>, ICallSiteILResolver
    {
        private class RuntimeContext
        {
            public object?[]? Constants;
            public ServiceFactory[]? Factories;
        }
        private struct ResolverMethod
        {
            public ServiceResolver Lambda;
            public RuntimeContext Context;
            public DynamicMethod DynamicMethod;
        }

        private ILogger<CallSiteILResolver> _Logger;
        private readonly Dictionary<ServiceCacheKey, ResolverMethod> _ScopeResolverCache;

        private static readonly FieldInfo _ConstantsField =
            typeof(RuntimeContext).GetField(nameof(RuntimeContext.Constants));
        private static readonly FieldInfo _FactoriesField =
            typeof(RuntimeContext).GetField(nameof(RuntimeContext.Factories));

        private static readonly MethodInfo _ServiceFactoryInvoke =
            typeof(ServiceFactory).GetMethod(nameof(ServiceFactory.Invoke)) ?? throw new InvalidOperationException();
        
        
        public ServiceResolver Build(ServiceCallSite callSite)
        {
            if (callSite.CacheInfo.Location != CacheLocation.Scope) return BuildResolver(callSite).Lambda;

            if (_ScopeResolverCache.TryGetValue(callSite.CacheInfo.CacheKey, out ResolverMethod resolver))
            {
                return resolver.Lambda;
            }
            
            resolver = BuildResolver(callSite);
            _ScopeResolverCache.Add(callSite.CacheInfo.CacheKey, resolver);
            return resolver.Lambda;

        }

        private ResolverMethod BuildResolver(ServiceCallSite callSite)
        {
            var method = new DynamicMethod($"ResolveService",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(object),
                new[] { typeof(RuntimeContext), typeof(ServiceProviderScope) },
                GetType(),
                true);
            
            ILGenerator generator = method.GetILGenerator();
            var context = new ILResolverContext(generator);

            if (callSite.CacheInfo.Location != CacheLocation.Scope)
            {
                VisitCallSiteCache(callSite, context);
                context.Generator.Emit(OpCodes.Ret);
            }
            else BuildScopedResolver(callSite, context);

            var runtimeContext = new RuntimeContext()
            {
                Constants = context.Constants?.ToArray(),
                Factories = context.Factories?.ToArray()
            };
            
            return new ResolverMethod()
            {
                Context = runtimeContext,
                DynamicMethod = method,
                Lambda = (ServiceResolver)method.CreateDelegate(typeof(ServiceResolver), runtimeContext)
            };
        }

        private void BuildScopedResolver(ServiceCallSite callSite, ILResolverContext context)
        {
            throw new NotImplementedException();
        }
        
        protected override object? VisitConstructor(ConstructorCallSite callSite, ILResolverContext context)
        {
            foreach (ServiceCallSite parameterCallSite in callSite.ParameterCallSites)
            {
                VisitCallSiteCache(parameterCallSite, context);
                if (parameterCallSite.ServiceType.IsValueType)
                {
                    context.Generator.Emit(OpCodes.Unbox_Any, parameterCallSite.ServiceType);
                }
            }
            context.Generator.Emit(OpCodes.Newobj, callSite.ConstructorInfo);
            if (callSite.ImplementationType!.IsValueType)
            {
                context.Generator.Emit(OpCodes.Unbox_Any, callSite.ImplementationType);
            }

            return null;
        }

        protected override object? VisitConstant(ConstantCallSite callSite, ILResolverContext context)
        {
            AddConstant(context, callSite.Value);
            return null;
        }

        protected override object? VisitServiceProvider(ServiceProviderCallSite callSite, ILResolverContext context)
        {
            throw new NotImplementedException();
        }

        protected override object? VisitEnumerable(EnumerableCallSite callSite, ILResolverContext context)
        {
            LocalBuilder resolvedServices = context.Generator.DeclareLocal(callSite.ServiceType.MakeArrayType());
            context.Generator.Emit(OpCodes.Ldc_I4, callSite.CallSites.Length);
            context.Generator.Emit(OpCodes.Newarr, callSite.SingleServiceType);
            context.Generator.Emit(OpCodes.Stloc, resolvedServices);

            for (var i = 0; i < callSite.CallSites.Length; i++)
            {
                ServiceCallSite currentCallSite = callSite.CallSites[i];
                context.Generator.Emit(OpCodes.Ldloc, resolvedServices);
                context.Generator.Emit(OpCodes.Ldc_I4, i);
                VisitCallSiteCache(currentCallSite, context);
                context.Generator.Emit(OpCodes.Stelem, callSite.SingleServiceType);
            }
            
            context.Generator.Emit(OpCodes.Ldloc, resolvedServices);
            return null;
        }

        protected override object? VisitFactory(FactoryCallSite callSite, ILResolverContext context)
        {
            context.Factories ??= new List<ServiceFactory>();
            
            context.Generator.Emit(OpCodes.Ldarg_0);
            context.Generator.Emit(OpCodes.Ldfld, _FactoriesField);
            context.Generator.Emit(OpCodes.Ldc_I4, context.Factories.Count);
            context.Generator.Emit(OpCodes.Ldelem, typeof(ServiceFactory));
            context.Generator.Emit(OpCodes.Ldarg_1);
            context.Generator.Emit(OpCodes.Callvirt, _ServiceFactoryInvoke);

            context.Factories.Add(callSite.Factory);
            return null;
        }

        private static void AddConstant(ILResolverContext context, object? constant)
        {
            context.Constants ??= new List<object?>();
            
            context.Generator.Emit(OpCodes.Ldarg_0);
            context.Generator.Emit(OpCodes.Ldfld, _ConstantsField);
            context.Generator.Emit(OpCodes.Ldc_I4, context.Constants.Count);
            context.Generator.Emit(OpCodes.Ldelem, typeof(object));
            context.Constants.Add(constant);
        }

        public CallSiteILResolver(ILogger<CallSiteILResolver> logger)
        {
            _ScopeResolverCache = new Dictionary<ServiceCacheKey, ResolverMethod>();
            _Logger = logger;
        }
    }
}