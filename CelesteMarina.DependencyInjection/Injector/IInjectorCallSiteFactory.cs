using System;

namespace CelesteMarina.DependencyInjection.Injector
{
    /// <summary>
    /// Provides retrieval of <see cref="InjectorCallSite">InjectorCallSites</see> by target type.
    /// </summary>
    internal interface IInjectorCallSiteFactory
    {
        /// <summary>
        /// Retrieve an <see cref="InjectorCallSite"/> by its target type.
        /// </summary>
        /// <param name="type">The target injected type of the call site.</param>
        /// <returns>An injector call site for <paramref name="type"/>.</returns>
        InjectorCallSite GetCallSite(Type type);
    }
}