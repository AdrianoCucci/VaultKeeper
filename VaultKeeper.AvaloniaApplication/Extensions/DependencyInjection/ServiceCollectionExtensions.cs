using Microsoft.Extensions.DependencyInjection;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Services;

namespace VaultKeeper.AvaloniaApplication.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAvaloniaServices(this IServiceCollection services) => services
        .AddSingleton<IApplicationService, ApplicationService>()
        .AddSingleton<IPlatformService, PlatformService>()
        .AddSingleton<IBackupService, BackupService>()
        .AddSingleton<IThemeService, ThemeService>();
}
