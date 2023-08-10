using System.Reflection.Emit;
using DIContainer.CallSite.Visitor;

namespace DIContainer.Injector.Visitor
{
    internal class ILInjectorContext
    {
        public ILResolverContext ResolverContext { get; }
        public ILGenerator Generator => ResolverContext.Generator;
        public LocalBuilder Instance { get; }

        public ILInjectorContext(ILResolverContext resolverContext, LocalBuilder instance)
        {
            ResolverContext = resolverContext;
            Instance = instance;
        }
    }
}