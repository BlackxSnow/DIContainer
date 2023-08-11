using DIContainer.Provider;

namespace DIContainer.CallSite.Visitor
{
    /// <summary>
    /// Provides immediate, runtime service resolution from <see cref="ServiceCallSite">ServiceCallSites</see>. 
    /// </summary>
    public interface ICallSiteRuntimeResolver
    {
        /// <summary>
        /// Attempts to immediately resolve the service described by <paramref name="callSite"/>.
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="CallSiteVisitor{TContext,TResult}.VisitCallSiteCache"/></param>
        /// <param name="scope">The scope to use for service resolution.</param>
        /// <returns>The resolved service on success, or null on failure.</returns>
        object? Resolve(ServiceCallSite callSite, ServiceProviderScope scope);
    }
}