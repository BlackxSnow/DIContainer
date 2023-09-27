using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Options;

namespace CelesteMarina.DependencyInjection.Extensions
{
    public static class ServiceCollectionOptionsExtensions
    {
        public static IServiceCollection AddOptions(this IServiceCollection services)
        {
            services.AddOnceSingleton(typeof(IOptions<>), typeof(OptionsManager<>));
            services.AddOnceScoped(typeof(IOptionsSnapshot<>), typeof(OptionsManager<>));
            services.AddOnceSingleton(typeof(IOptionsMonitor<>), typeof(OptionsMonitor<>));
            services.AddOnceTransient(typeof(IOptionsFactory<>), typeof(OptionsFactory<>));
            services.AddOnceSingleton(typeof(IOptionsMonitorCache<>), typeof(OptionsCache<>));
            return services;
        }
    }
}