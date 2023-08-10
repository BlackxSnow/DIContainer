using System;
using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    internal interface ICallSiteILResolver
    {
        ServiceResolver Build(ServiceCallSite callSite);
        void BuildInline(ServiceCallSite callSite, ILResolverContext context);
    }
}