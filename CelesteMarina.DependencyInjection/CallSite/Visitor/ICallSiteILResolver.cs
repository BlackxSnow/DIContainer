using System;
using CelesteMarina.DependencyInjection.Injector.Visitor;
using CelesteMarina.DependencyInjection.Provider;
using IServiceProvider = CelesteMarina.DependencyInjection.Provider.IServiceProvider;

namespace CelesteMarina.DependencyInjection.CallSite.Visitor
{
    /// <summary>
    /// Provides service resolution from <see cref="ServiceCallSite">ServiceCallSites</see> via dynamic Intermediate Language code. 
    /// </summary>
    internal interface ICallSiteILResolver
    {
        IInjectorILResolver ILInjector { get; }
        /// <summary>
        /// Constructs a <see cref="ServiceResolver"/> delegate for <paramref name="callSite"/>.
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="CallSiteVisitor{TContext,TResult}.VisitCallSiteCache"/></param>
        /// <returns>A delegate that returns the service described by <paramref name="callSite"/>.</returns>
        ServiceResolver Build(ServiceCallSite callSite);
        /// <summary>
        /// Appends service resolution IL instructions for <paramref name="callSite"/> to <paramref name="context"/>. 
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="Build"/></param>
        /// <param name="context">The context to append to.</param>
        void BuildInline(ServiceCallSite callSite, ILResolverContext context);
        
        void OnInitialisationComplete(IServiceProvider provider);
    }
}