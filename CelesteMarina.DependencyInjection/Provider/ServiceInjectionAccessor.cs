using System;
using CelesteMarina.DependencyInjection.Injector;

namespace CelesteMarina.DependencyInjection.Provider
{
    public class ServiceInjectionAccessor
    {
        public InjectorCallSite CallSite { get; }
        public ServiceInjector Injector { get; set; }

        public ServiceInjectionAccessor(InjectorCallSite callSite, ServiceInjector injector)
        {
            CallSite = callSite;
            Injector = injector;
        }
    }
}