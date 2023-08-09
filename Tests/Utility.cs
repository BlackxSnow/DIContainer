using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Tests
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
    }
}