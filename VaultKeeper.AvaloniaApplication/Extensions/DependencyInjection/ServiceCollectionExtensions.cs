using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Services;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNavigator(this IServiceCollection services, Func<IServiceProvider, IEnumerable<Route>> routesFunc, string? defaultRouteKey = null)
    {
        return services.AddSingleton<INavigator>(sp => new Navigator(sp.GetRequiredService<ILogger<Navigator>>(), routesFunc.Invoke(sp), defaultRouteKey));
    }
}
