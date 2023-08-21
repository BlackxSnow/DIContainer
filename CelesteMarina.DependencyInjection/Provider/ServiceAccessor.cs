using System;
using CelesteMarina.DependencyInjection.CallSite;

namespace CelesteMarina.DependencyInjection.Provider
{
    public class ServiceAccessor
    {
        public ServiceCallSite? CallSite { get; set; }
        public ServiceResolver? Resolver;
    }
}