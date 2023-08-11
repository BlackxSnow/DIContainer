using System;
using DIContainer.Injector.Visitor;
using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    internal interface ICallSiteILResolver
    {
        IInjectorILResolver ILInjector { get; }
        ServiceResolver Build(ServiceCallSite callSite);
        void BuildInline(ServiceCallSite callSite, ILResolverContext context);
    }
}