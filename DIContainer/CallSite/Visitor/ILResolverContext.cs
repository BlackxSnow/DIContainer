using System.Collections.Generic;
using System.Reflection.Emit;

namespace DIContainer.CallSite.Visitor
{
    public struct ILResolverContext
    {
        public ILGenerator Generator { get; }
        public List<object?>? Constants { get; set; }
        public List<ServiceFactory>? Factories { get; set; }
    }
}