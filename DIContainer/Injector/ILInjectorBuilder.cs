using System;
using DIContainer.CallSite;
using DIContainer.CallSite.Visitor;

namespace DIContainer.Injector
{
    public class ILInjectorBuilder
    {
        public void Build(InjectorCallSite callSite, ILResolverContext context,
            Func<ServiceCallSite, ILResolverContext, object?> visitCallSite)
        {
            throw new NotImplementedException();
        }

        public ServiceInjector BuildDelegate(InjectorCallSite callSite)
        {
            throw new NotImplementedException();
        }
    }
}