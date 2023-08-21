using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.Provider.Temporary
{
    public class TemporaryServiceCollection : ServiceCollection, ITemporaryServiceCollection
    {
        public TemporaryServiceCollection(params ServiceDescriptor[] services)
        {
            Services.AddRange(services);
        }
    }
}