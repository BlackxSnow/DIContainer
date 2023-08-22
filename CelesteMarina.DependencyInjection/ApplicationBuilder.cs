using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection
{
    public class ApplicationBuilder
    {
        public IServiceCollection Services { get; }

        public IServiceProvider Build()
        {
            var provider = new ServiceProvider(Services);
            return provider;
        }
        
        public ApplicationBuilder()
        {
            Services = new ServiceCollection();
        }
    }
}