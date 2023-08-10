using System.Reflection.Emit;
using DIContainer.CallSite.Visitor;

namespace DIContainer.Injector.Visitor
{
    internal interface IInjectorILResolver
    {
        void Build(InjectorCallSite callSite, ILInjectorContext context);

        ServiceInjector BuildDelegate(InjectorCallSite callSite);
    }
}