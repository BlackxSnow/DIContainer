using System;

namespace CelesteMarina.DependencyInjection.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsConstructable(this Type type)
        {
            return !(type.IsGenericTypeDefinition || type.IsAbstract || type.IsInterface);
        }
    }
}