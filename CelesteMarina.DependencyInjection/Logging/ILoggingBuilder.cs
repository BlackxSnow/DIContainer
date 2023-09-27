using CelesteMarina.DependencyInjection.Service;

namespace CelesteMarina.DependencyInjection.Logging
{
    public interface ILoggingBuilder
    {
        IServiceCollection Services { get; }
    }
}