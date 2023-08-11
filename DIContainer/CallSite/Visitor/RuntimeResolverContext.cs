using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    /// <summary>
    /// Contains data used during <see cref="CallSiteRuntimeResolver"/> operations.
    /// </summary>
    internal struct RuntimeResolverContext
    {
        public ServiceProviderScope Scope { get; set; }
    }
}