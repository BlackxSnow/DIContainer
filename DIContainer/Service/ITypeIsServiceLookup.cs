using System;

namespace DIContainer.Service
{
    public interface ITypeIsServiceLookup
    {
        bool IsService(Type type);
    }
}