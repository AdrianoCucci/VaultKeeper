using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.Services;

public class Navigator : INavigator
{
    public event EventHandler<CurrentRoute>? Navigated;

    private readonly ILogger<Navigator> _logger;
    private readonly Dictionary<string, Route> _routes;

    private CurrentRoute _currentRoute;
    public CurrentRoute CurrentRoute => _currentRoute;

    public Navigator(ILogger<Navigator> logger, IEnumerable<Route> routes, string? defaultRouteKey = null)
    {
        if (routes.Select(x => x.Key).Distinct().Count() < routes.Count())
            throw new ArgumentException("Route collection cannot have duplicate Keys.", nameof(routes));

        _logger = logger;
        _routes = routes.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.First());

        if (!string.IsNullOrWhiteSpace(defaultRouteKey) && _routes.TryGetValue(defaultRouteKey, out Route? route))
            _currentRoute = new(route);
        else
            _currentRoute = new(_routes.First().Value);
    }

    public IEnumerable<Route> FindRoutes(Func<Route, bool> predicate)
    {
        return _routes
            .Where(kvp => predicate.Invoke(kvp.Value))
            .Select(kvp => kvp.Value);
    }

    public bool Navigate(string routeKey, Dictionary<string, object?>? routeParams = null)
    {
        _logger.LogInformation($"{nameof(Navigate)} | key: \"{{key}}\"", routeKey);

        if (!_routes.TryGetValue(routeKey, out Route? route))
        {
            _logger.LogWarning("Attempted to navigate to a route with a key that does not exist: \"{key}\"", routeKey);
            return false;
        }

        _currentRoute = new(route) { Params = routeParams ?? [] };
        Navigated?.Invoke(this, _currentRoute);

        return true;
    }
}