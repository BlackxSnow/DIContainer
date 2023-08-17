using DIContainer.Service;

namespace DIContainer.Provider.Temporary
{
    public class TemporaryServiceCollection : ServiceCollection, ITemporaryServiceCollection
    {
        public TemporaryServiceCollection(params ServiceDescriptor[] services)
        {
            Services.AddRange(services);
        }
    }
}