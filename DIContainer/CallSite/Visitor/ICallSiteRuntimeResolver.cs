using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    public interface ICallSiteRuntimeResolver
    {
        object? Resolve(ServiceCallSite callSite, ServiceProviderScope scope);
    }
}