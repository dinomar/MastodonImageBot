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

        public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder, Action<ColorConsoleLoggerConfiguration> configure)
        {
            builder.AddColorConsoleLogger();
            builder.Services.Configure(configure);

            return builder;
        }

        //public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder, Func<ColorConsoleLoggerConfiguration> configuration)
        //{
        //    builder.Services.AddSingleton<ILoggerProvider, ColorConsoleLoggerProvider>(p => new ColorConsoleLoggerProvider(configuration));
        //    return builder;
        //}


    }
}
