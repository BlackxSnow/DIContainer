using System;
using System.Collections.Generic;

namespace DIContainer.Injector
{
    public class InjectorCallSiteFactory
    {
        private Dictionary<Type, InjectorCallSite> _CallSiteCache;

        public InjectorCallSite GetCallSite(Type type)
        {
            throw new NotImplementedException();
        }
        
        private InjectorCallSite BuildCallSite(Type type)
        {
            throw new NotImplementedException();
        }
        
        private MethodInjectionPoint? GetMethodInjectionPoint(Type type)
        {
            throw new NotImplementedException();
        }
        
        private PropertyInjectionPoint[]? GetPropertyInjectionPoints(Type type)
        {
            throw new NotImplementedException();
        }
    }
}