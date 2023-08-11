using System.Reflection;
using System.Reflection.Emit;

namespace DIContainer.Utility
{
    internal static class IL
    {
        public class RuntimeContext
        {
            public object?[]? Constants;
            public ServiceFactory[]? Factories;
        }
        
        internal static readonly FieldInfo RuntimeContextConstants =
            typeof(IL.RuntimeContext).GetField(nameof(IL.RuntimeContext.Constants));
        internal static readonly FieldInfo RuntimeContextFactories =
            typeof(IL.RuntimeContext).GetField(nameof(IL.RuntimeContext.Factories));
    }
}