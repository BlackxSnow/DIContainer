using System.Collections.Generic;
using System.Reflection.Emit;

namespace DIContainer.CallSite.Visitor
{
    internal class ILResolverContext
    {
        public ILGenerator Generator { get; }
        public List<object?>? Constants;
        public List<ServiceFactory>? Factories;

        public ILResolverContext(ILGenerator generator)
        {
            Generator = generator;
        }
    }
}