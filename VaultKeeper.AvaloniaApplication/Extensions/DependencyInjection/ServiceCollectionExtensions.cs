using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.Navigation;
using VaultKeeper.AvaloniaApplication.Services;
using VaultKeeper.AvaloniaApplication.Services.Navigation;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAvaloniaServices(this IServiceCollection services) => services
        .AddSingleton<IApplicationService, ApplicationService>()
        .AddSingleton<IPlatformService, PlatformService>()
        .AddSingleton<IThemeService, ThemeService>();

    public static IServiceCollection AddNavigation(
        this IServiceCollection services,
        Func<IServiceProvider, IEnumerable<RouteScope>> routesFunc)
    {
        return services.AddSingleton<INavigatorFactory>(sp => new NavigatorFactory(sp, routesFunc.Invoke(sp)));
    }
}
