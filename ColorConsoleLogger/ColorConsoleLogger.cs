using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorConsoleLogger
{
    public sealed class ColorConsoleLogger : ILogger
    {
        private readonly string _name;

        private readonly Func<ColorConsoleLoggerConfiguration> _getCurrentConfig;

        public ColorConsoleLogger(string name, Func<ColorConsoleLoggerConfiguration> getCurrentConfig)
        {
            _name = name;
            _getCurrentConfig = getCurrentConfig;
        }

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => _getCurrentConfig().LogLevels.ContainsKey(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            ColorConsoleLoggerConfiguration config = _getCurrentConfig();

            ConsoleColor originalColor = Console.ForegroundColor;
            ConsoleColor textColor = config.LogLevels[logLevel];

            string message = formatter(state, exception);

            Console.ForegroundColor = textColor;
            if (config.IncludeNamePrefix)
            {
                Console.WriteLine($"{_name}: {message}");
            }
            else
            {
                Console.WriteLine(message);
            }
            
            Console.ForegroundColor = originalColor;

            if (exception != null)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
