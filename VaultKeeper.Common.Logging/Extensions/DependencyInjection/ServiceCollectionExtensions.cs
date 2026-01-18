using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace VaultKeeper.Common.Logging.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureLogging(this IServiceCollection services, LoggingConfig config) => services.AddLogging(builder =>
    {
        const string outputTemplate = "{Timestamp:HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}";

        LoggerConfiguration configuration = new LoggerConfiguration()
            .MinimumLevel.Is((LogEventLevel)config.LogLevel)
            .Enrich.FromLogContext();

#if DEBUG
        configuration = configuration.WriteTo.Debug(outputTemplate: outputTemplate);
#endif
        if (config.FileLoggingConfig is FileLoggingConfig fileConfig)
        {
            configuration.WriteTo.File(
                fileConfig.FilePath,
                outputTemplate: outputTemplate,
                rollingInterval: (RollingInterval)fileConfig.RollingInterval,
                retainedFileCountLimit: fileConfig.FileLimit);
        }

        Logger logger = configuration.CreateLogger();

        builder.AddSerilog(logger, true);
    });
}
