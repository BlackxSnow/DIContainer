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
    }
}