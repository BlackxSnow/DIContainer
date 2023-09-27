using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.Logging
{
    public class LoggingBuilder : ILoggingBuilder
    {
        public IServiceCollection Services { get; }

        public LoggingBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}