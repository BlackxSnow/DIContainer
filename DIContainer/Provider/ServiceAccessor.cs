using System;
using DIContainer.CallSite;

namespace DIContainer.Provider
{
    public class ServiceAccessor
    {
        public ServiceCallSite? CallSite { get; set; }
        public ServiceResolver? Resolver;
    }
}