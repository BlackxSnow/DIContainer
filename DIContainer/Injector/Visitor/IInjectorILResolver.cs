using System.Reflection.Emit;
using DIContainer.CallSite.Visitor;

namespace DIContainer.Injector.Visitor
{
    /// <summary>
    /// Provides dependency injection for object instances through <see cref="InjectionAttribute"/> properties and methods.
    /// Generates dynamic intermediate language code.
    /// </summary>
    internal interface IInjectorILResolver
    {
        /// <summary>
        /// Appends injection IL instructions for <paramref name="callSite"/> to <paramref name="context"/>.
        /// </summary>
        /// <param name="callSite">The call site for the instance type to inject into.</param>
        /// <param name="context">The context to append to.</param>
        void Build(InjectorCallSite callSite, ILInjectorContext context);

        /// <summary>
        /// Builds a delegate for injection into instances of the <paramref name="callSite"/> target type.
        /// </summary>
        /// <param name="callSite"><inheritdoc cref="Build"/></param>
        /// <returns>A delegate for injecting dependencies into objects.</returns>
        ServiceInjector BuildDelegate(InjectorCallSite callSite);
    }
}