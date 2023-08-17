using System.Collections.Generic;
using System.Reflection.Emit;

namespace DIContainer.CallSite.Visitor
{
    /// <summary>
    /// Contains data used during <see cref="CallSiteILResolver"/> operations.
    /// </summary>
    internal class ILResolverContext
    {
        public ILGenerator Generator { get; }
        /// <summary>
        /// Constant values for access by the IL instructions.
        /// </summary>
        public List<object?>? Constants;
        /// <summary>
        /// Factory delegates for access by the IL instructions.
        /// </summary>
        public List<ServiceFactory>? Factories;

        public ILResolverContext(ILGenerator generator)
        {
            Generator = generator;
        }
    }
}