using Serilog;
using Serilog.Events;
using System.Globalization;

namespace Bixa.Backend.Configuration;

public static class SerilogConfig
{
    public static void ConfigureSerilog(HostBuilderContext hostContext, LoggerConfiguration loggerConfiguration)
    {
        var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        if (!Directory.Exists(logsDirectory))
            Directory.CreateDirectory(logsDirectory);

        var configuration = hostContext.Configuration;
        bool isDev = configuration.GetValue<bool>("ShowInformationLog");
        var minimumLevel = configuration.GetValue<LogEventLevel>("Serilog:MinimumLevel:Default");
        var outputTemplate = configuration.GetValue<string>("Serilog:WriteTo:0:Args:outputTemplate")
            ?? "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        var rollingInterval = configuration.GetValue<RollingInterval>("Serilog:WriteTo:0:Args:rollingInterval");
        var fileSizeLimitBytes = configuration.GetValue<long>("Serilog:WriteTo:0:Args:fileSizeLimitBytes");
        var retainedFileCountLimit = configuration.GetValue<int>("Serilog:WriteTo:0:Args:retainedFileCountLimit");

        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
             .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(logEvent => logEvent.Level == LogEventLevel.Warning)
                .WriteTo.Console(restrictedToMinimumLevel: isDev ? LogEventLevel.Information : LogEventLevel.Error)
            )
            .Enrich.WithProperty("ApplicationName", hostContext.HostingEnvironment.ApplicationName)
            .Enrich.WithProperty("EnvironmentName", hostContext.HostingEnvironment.EnvironmentName)
            .MinimumLevel.Is(minimumLevel)
            .WriteTo.File(
                path: Path.Combine(logsDirectory, $"log-{DateTime.UtcNow:yyyyMMdd}-W{ISOWeek.GetWeekOfYear(DateTime.UtcNow)}.txt"),
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: outputTemplate,
                fileSizeLimitBytes: fileSizeLimitBytes,
                rollingInterval: rollingInterval,
                retainedFileCountLimit: retainedFileCountLimit);
    }
}