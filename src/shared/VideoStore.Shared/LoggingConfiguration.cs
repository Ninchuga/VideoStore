using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

namespace VideoStore.Shared
{
    public static class LoggingConfiguration
    {
        /// <summary>
        /// Used in Program.cs to configure logger on host builder 
        /// </summary>
        public static ILogger CreateLogger(IHostEnvironment environment)
        {
            var levelSwitch = new LoggingLevelSwitch();

            if (environment.IsDevelopment())
            {
                //loggerConfiguration.MinimumLevel.Override("*.API", LogEventLevel.Debug);
                levelSwitch.MinimumLevel = LogEventLevel.Debug; // place this maybe in appsettings
            }

            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch) // <= default minimum level
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", environment.ApplicationName)
                .Enrich.WithProperty("EnvironmentName", environment.EnvironmentName)
                .Enrich.WithExceptionDetails()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // <= overrides namespaces to log from Warning to higher (debug, info will be skipped)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
