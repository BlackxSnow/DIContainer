using DIContainer.Service;

namespace DIContainer.CallSite
{
    internal interface ICallSiteFactory
    {
        ServiceCallSite? GetCallSite(ServiceIdentifier identifier);
    }
}