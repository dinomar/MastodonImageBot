using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorConsoleLogger
{
    public class ColorConsoleLoggerConfiguration
    {
        public bool IncludeNamePrefix { get; set; } = true;

        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; set; } = new Dictionary<LogLevel, ConsoleColor>
        {
            [LogLevel.Debug] = ConsoleColor.White,
            [LogLevel.Information] = ConsoleColor.Green,
            [LogLevel.Warning] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red
        };
    }
}
