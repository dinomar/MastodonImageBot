using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorConsoleLogger
{
    public static class ColorConsoleLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, ColorConsoleLoggerProvider>();
            return builder;
        }
    }
}
