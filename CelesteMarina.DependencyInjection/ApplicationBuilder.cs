using System;
using CelesteMarina.DependencyInjection.Provider;
using CelesteMarina.DependencyInjection.Service;
using Microsoft.Extensions.Logging;
using IServiceProvider = CelesteMarina.DependencyInjection.Provider.IServiceProvider;

namespace CelesteMarina.DependencyInjection
{
    public class ApplicationBuilder
    {
        public IServiceCollection Services { get; }
        private Action<ILoggingBuilder> _InitialisationLoggingBuilder;

        public void ConfigureInitialisationLogger(Action<ILoggingBuilder> configure)
        {
            _InitialisationLoggingBuilder = configure;
        }
        
        public IServiceProvider Build()
        {
            var provider = new ServiceProvider(Services, LoggerFactory.Create(_InitialisationLoggingBuilder));
            return provider;
        }
        
        public ApplicationBuilder()
        {
            _InitialisationLoggingBuilder = b => b.AddConsole();
            Services = new ServiceCollection();
        }
    }
}