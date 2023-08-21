using System;

namespace CelesteMarina.DependencyInjection.Service
{
    public interface ITypeIsServiceLookup
    {
        bool IsService(Type type);
    }
}