using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CelesteMarina.DependencyInjection;
using CelesteMarina.DependencyInjection.CallSite;
using CelesteMarina.DependencyInjection.CallSite.Visitor;
using CelesteMarina.DependencyInjection.Injector.Visitor;
using CelesteMarina.DependencyInjection.Service;
using CelesteMarina.DependencyInjection.Utility;

namespace CelesteMarina.DependencyInjection.Tests.Mock
{
    internal class CallSiteILResolverMock : ICallSiteILResolver
    {
        private readonly Dictionary<ServiceCacheKey, ConstantCallSite> _Constants;

        public IInjectorILResolver ILInjector { get; }
        public ServiceResolver Build(ServiceCallSite callSite)
        {
            throw new NotSupportedException();
        }

        public void BuildInline(ServiceCallSite callSite, ILResolverContext context)
        {
            if (callSite.Kind != CallSiteKind.Constant) throw new NotSupportedException();

            if (!_Constants.TryGetValue(callSite.CacheInfo.CacheKey, out ConstantCallSite constant))
            {
                throw new ArgumentOutOfRangeException();
            }
            
            AddConstant(context, constant.Value);
        }

        private static void AddConstant(ILResolverContext context, object? constant)
        {
            context.Constants ??= new List<object?>();
            
            context.Generator.Emit(OpCodes.Ldarg_0);
            context.Generator.Emit(OpCodes.Ldfld, IL.RuntimeContextConstants);
            context.Generator.Emit(OpCodes.Ldc_I4, context.Constants.Count);
            context.Generator.Emit(OpCodes.Ldelem, typeof(object));
            context.Constants.Add(constant);
        }
        
        public CallSiteILResolverMock(params ConstantCallSite[] constants)
        {
            _Constants = constants.ToDictionary(c => c.CacheInfo.CacheKey);
            ILInjector = new InjectorILResolver(this);
        }
    }
}