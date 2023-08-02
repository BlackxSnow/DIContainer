using System;
using DIContainer.Injector;
using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    public class ILResolverBuilder
    {
        private InjectorILBuilder _InjectorILBuilder;

        public Func<ServiceProviderScope, object?> Build(ServiceCallSite callSite)
        {
            throw new NotImplementedException();
        }
    }
}