using System.Collections.Generic;
using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.CallSite
{
    /// <summary>
    /// Provides retrieval of <see cref="ServiceCallSite">ServiceCallSites</see> by <see cref="ServiceIdentifier">ServiceIdentifiers</see>.
    /// </summary>
    internal interface ICallSiteFactory : ITypeIsServiceLookup
    {
        /// <summary>
        /// Attempt to retrieve the <see cref="ServiceCallSite"/> represented by <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">The identifying key for the desired <see cref="ServiceCallSite"/>.</param>
        /// <returns>The call site on success, or null on failure.</returns>
        ServiceCallSite? GetCallSite(ServiceIdentifier identifier);

        void AddServices(IEnumerable<ServiceDescriptor> services);
        void RemoveServices(IEnumerable<ServiceDescriptor> services);
    }
}