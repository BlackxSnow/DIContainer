using System;
using System.Collections.Generic;
using DIContainer.Provider.Temporary;

namespace DIContainer.Service
{
    public struct ServiceDescriptor
    {
        public Type ServiceType { get; }
        public ServiceLifetime Lifetime { get; }
        public ITemporaryServiceContainer? Container { get; }
        public Type? ImplementationType { get; }
        public object? ImplementationInstance { get; }
        public ServiceFactory? ImplementationFactory { get; }
        public IReadOnlyCollection<Type>? ImplementationNotableTypes { get; }
    }
}