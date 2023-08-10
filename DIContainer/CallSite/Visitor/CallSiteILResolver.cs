using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DIContainer.Injector;
using DIContainer.Injector.Visitor;
using DIContainer.Provider;
using DIContainer.Service;
using DIContainer.Utility;
using Microsoft.Extensions.Logging;

namespace DIContainer.CallSite.Visitor
{
    internal class CallSiteILResolver : CallSiteVisitor<ILResolverContext, object?>, ICallSiteILResolver
    {
        private struct ResolverMethod
        {
            public ServiceResolver Lambda;
            public IL.RuntimeContext Context;
            public DynamicMethod DynamicMethod;
        }

        private ILogger<CallSiteILResolver> _Logger;
        private readonly Dictionary<ServiceCacheKey, ResolverMethod> _ScopeResolverCache;
        private ServiceProviderScope _RootScope;
        private ICallSiteRuntimeResolver _RuntimeResolver;
        public IInjectorILResolver ILInjector { get; }

        private static readonly FieldInfo _ConstantsField =
            typeof(IL.RuntimeContext).GetField(nameof(IL.RuntimeContext.Constants));
        private static readonly FieldInfo _FactoriesField =
            typeof(IL.RuntimeContext).GetField(nameof(IL.RuntimeContext.Factories));

        private static readonly MethodInfo _ServiceFactoryInvoke =
            typeof(ServiceFactory).GetMethod(nameof(ServiceFactory.Invoke))!;

        private static readonly FieldInfo _ScopeResolvedServices =
            typeof(ServiceProviderScope).GetField(nameof(ServiceProviderScope.ResolvedServices),
                BindingFlags.Instance | BindingFlags.NonPublic)!;

        private static readonly MethodInfo _ScopeCaptureDisposable =
            typeof(ServiceProviderScope).GetMethod(nameof(ServiceProviderScope.CaptureIfDisposable))!;

        private static readonly MethodInfo _ServiceCacheTryGet =
            typeof(Dictionary<ServiceCacheKey, object>).GetMethod(
                nameof(Dictionary<ServiceCacheKey, object>.TryGetValue))!;

        private static readonly MethodInfo _ServiceCacheAdd =
            typeof(Dictionary<ServiceCacheKey, object>).GetMethod(nameof(Dictionary<ServiceCacheKey, object>.Add))!;
        
        
        public ServiceResolver Build(ServiceCallSite callSite)
        {
            return GetOrBuildResolver(callSite).Lambda;
        }

        public void BuildInline(ServiceCallSite callSite, ILResolverContext context)
        {
            VisitCallSiteCache(callSite, context);
        }
        
        private ResolverMethod GetOrBuildResolver(ServiceCallSite callSite)
        {
            if (callSite.CacheInfo.Location != CacheLocation.Scope) return BuildResolver(callSite);
            
            if (_ScopeResolverCache.TryGetValue(callSite.CacheInfo.CacheKey, out ResolverMethod resolver))
            {
                return resolver;
            }
            
            resolver = BuildResolver(callSite);
            _ScopeResolverCache.Add(callSite.CacheInfo.CacheKey, resolver);
            return resolver;
        }
        
        private ResolverMethod BuildResolver(ServiceCallSite callSite)
        {
            var method = new DynamicMethod($"ResolveService",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(object),
                new[] { typeof(IL.RuntimeContext), typeof(ServiceProviderScope) },
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

            var runtimeContext = new IL.RuntimeContext()
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
            LocalBuilder scopeServices = context.Generator.DeclareLocal(typeof(Dictionary<ServiceCacheKey, object?>));
            LocalBuilder cacheKey = context.Generator.DeclareLocal(typeof(ServiceCacheKey));
            LocalBuilder service = context.Generator.DeclareLocal(typeof(object));
            
            Label returnLabel = context.Generator.DefineLabel();
            
            // scopeServices = Scope.ResolvedServices
            context.Generator.Emit(OpCodes.Ldarg_1);
            context.Generator.Emit(OpCodes.Ldfld, _ScopeResolvedServices);
            context.Generator.Emit(OpCodes.Stloc, scopeServices);
            
            AddConstant(context, callSite.CacheInfo.CacheKey);
            context.Generator.Emit(OpCodes.Unbox_Any, typeof(ServiceCacheKey));
            context.Generator.Emit(OpCodes.Stloc, cacheKey);
            
            // if(!scopeServices.TryGetValue(cacheKey, out service))
            context.Generator.Emit(OpCodes.Ldloc, scopeServices);
            context.Generator.Emit(OpCodes.Ldloc, cacheKey);
            context.Generator.Emit(OpCodes.Ldloca, service);
            context.Generator.Emit(OpCodes.Callvirt, _ServiceCacheTryGet);
            context.Generator.Emit(OpCodes.Brtrue, returnLabel);
            // {
            VisitCallSite(callSite, context);
            context.Generator.Emit(OpCodes.Stloc, service);
            
            // scopeServices.Add(cacheKey, service)
            context.Generator.Emit(OpCodes.Ldloc, scopeServices);
            context.Generator.Emit(OpCodes.Ldloc, cacheKey);
            context.Generator.Emit(OpCodes.Ldloc, service);
            context.Generator.Emit(OpCodes.Callvirt, _ServiceCacheAdd);

            if (callSite.IsTypeDisposable)
            {
                // scope.CaptureIfDisposable(service)
                context.Generator.Emit(OpCodes.Ldarg_1);
                context.Generator.Emit(OpCodes.Ldloc, service);
                context.Generator.Emit(OpCodes.Callvirt, _ScopeCaptureDisposable);
                context.Generator.Emit(OpCodes.Pop);
            }
            // }
            
            context.Generator.MarkLabel(returnLabel);
            context.Generator.Emit(OpCodes.Ldloc, service);
            context.Generator.Emit(OpCodes.Ret);
        }

        protected override object? VisitRootCache(ServiceCallSite callSite, ILResolverContext context)
        {
            AddConstant(context, _RuntimeResolver.Resolve(callSite, _RootScope));
            return null;
        }

        protected override object? VisitScopeCache(ServiceCallSite callSite, ILResolverContext context)
        {
            ResolverMethod resolver = GetOrBuildResolver(callSite);
            
            AddConstant(context, resolver.Context);
            context.Generator.Emit(OpCodes.Ldarg_1);
            context.Generator.Emit(OpCodes.Call, resolver.DynamicMethod);
            return null;
        }

        protected override object? VisitDisposeCache(ServiceCallSite callSite, ILResolverContext context)
        {
            if (callSite.IsTypeDisposable)
            {
                context.Generator.Emit(OpCodes.Ldarg_1);
                VisitCallSite(callSite, context);
                context.Generator.Emit(OpCodes.Callvirt, _ScopeCaptureDisposable);
                return null;
            }

            VisitCallSite(callSite, context);
            return null;
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
            
            if (callSite.InjectorCallSite != null)
            {
                LocalBuilder instance = context.Generator.DeclareLocal(typeof(object));
                context.Generator.Emit(OpCodes.Stloc, instance);
                ILInjector.Build(callSite.InjectorCallSite, new ILInjectorContext(context, instance));
                context.Generator.Emit(OpCodes.Ldloc, instance);
            }
            
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
            context.Generator.Emit(OpCodes.Ldarg_1);
            return null;
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

        public CallSiteILResolver(ILogger<CallSiteILResolver> logger, ServiceProviderScope rootScope, 
            ICallSiteRuntimeResolver runtimeResolver, Func<CallSiteILResolver, IInjectorILResolver> injectorBuilder)
        {
            _ScopeResolverCache = new Dictionary<ServiceCacheKey, ResolverMethod>();
            _Logger = logger;
            _RuntimeResolver = runtimeResolver;
            _RootScope = rootScope;
            ILInjector = injectorBuilder(this);
        }
    }
}