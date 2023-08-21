using System.Reflection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CelesteMarina.DependencyInjection.Tests
{
    public static class Utility
    {
        public static ILoggerFactory GetLoggerFactory(ITestOutputHelper testOutputHelper)
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
                builder.AddFilter("*", LogLevel.Trace);
            });
        }
        
        public const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    }
}