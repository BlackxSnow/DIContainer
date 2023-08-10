using System;
using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    public interface ICallSiteILResolver
    {
        ServiceResolver Build(ServiceCallSite callSite);
    }
}