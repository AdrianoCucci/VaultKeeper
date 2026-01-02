using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VaultKeeper.Models.Navigation;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.Services.Navigation;

public class Navigator : INavigator
{
    public event EventHandler<CurrentRoute>? Navigated;
    public string ScopeKey { get; }

    private readonly ILogger<Navigator> _logger;
    private readonly RouteScope _routeScope;

    private CurrentRoute _currentRoute;
    public CurrentRoute CurrentRoute => _currentRoute;

    public Navigator(ILogger<Navigator> logger, RouteScope routeScope)
    {
        _logger = logger;
        _routeScope = routeScope;
        _currentRoute = new(_routeScope.Key, _routeScope.Routes.First());
        ScopeKey = _routeScope.Key;
    }

    public CurrentRoute Navigate(string routeKey, Dictionary<string, object?>? routeParams = null)
    {
        _logger.LogInformation($"{nameof(Navigate)} | scope: {{scopeKey}} | route: {{key}}", ScopeKey, routeKey);

        if (_currentRoute.Key == routeKey)
            return _currentRoute;

        Route? newRoute = _routeScope.Routes.FirstOrDefault(x => x.Key == routeKey);
        if (newRoute == null)
        {
            _logger.LogWarning($"Attempted to navigate to a {nameof(Route)} key, that does not exist under {nameof(RouteScope)}\"{{scopeKey}}\": \"{{routeKey}}\"", ScopeKey, routeKey);
            return _currentRoute;
        }

        _currentRoute = new(ScopeKey, newRoute)
        {
            Params = routeParams ?? []
        };

        Navigated?.Invoke(this, _currentRoute);

        return _currentRoute;
    }

    public CurrentRoute NavigateToDefaultRoute()
    {
        _logger.LogInformation(nameof(NavigateToDefaultRoute));
        return Navigate(_routeScope.Routes.First().Key);
    }
}
