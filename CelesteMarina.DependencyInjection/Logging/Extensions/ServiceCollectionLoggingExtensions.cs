using System;
using CelesteMarina.DependencyInjection.Extensions;
using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CelesteMarina.DependencyInjection.Logging.Extensions
{
    public static class ServiceCollectionLoggingExtensions
    {
        public static IServiceCollection AddLogging(this IServiceCollection services, Action<ILoggingBuilder> configure)
        {
            services.AddOptions();
            
            services.AddOnceSingleton<ILoggerFactory, LoggerFactory>();
            services.AddOnceSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddOnceEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<LoggerFilterOptions>>(
                new ConfigureOptions<LoggerFilterOptions>(opts => opts.MinLevel = LogLevel.Information)));

            configure(new LoggingBuilder(services));
            return services;
        }
    }
}