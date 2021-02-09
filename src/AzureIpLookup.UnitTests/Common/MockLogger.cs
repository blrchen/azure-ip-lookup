using System;
using Microsoft.Extensions.Logging;

namespace AzureIpLookup.UnitTests.Common
{
    public class MockLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"[{logLevel} {eventId.Id}] {formatter(state, exception)}");
        }
    }
}