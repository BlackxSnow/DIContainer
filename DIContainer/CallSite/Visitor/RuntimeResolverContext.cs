using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    internal struct RuntimeResolverContext
    {
        public ServiceProviderScope Scope { get; set; }
    }
}