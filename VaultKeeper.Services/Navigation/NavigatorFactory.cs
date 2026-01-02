using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Navigation;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.Services.Navigation;

public class NavigatorFactory : INavigatorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NavigatorFactory> _logger;
    private readonly Dictionary<string, RouteScope> _routeScopesDict;
    private readonly Dictionary<string, Navigator> _navigatorScopesDict = [];

    public NavigatorFactory(IServiceProvider serviceProvider, IEnumerable<RouteScope> routeScopes)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<NavigatorFactory>>();

        var validateRouteScopesResult = ValidateRouteScopes(routeScopes);
        if (!validateRouteScopesResult.IsSuccessful)
        {
            validateRouteScopesResult.Logged(_logger);
            throw new ArgumentException(validateRouteScopesResult.Message, nameof(routeScopes));
        }

        _routeScopesDict = routeScopes.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.First());
    }

    private Result ValidateRouteScopes(IEnumerable<RouteScope> routeScopes)
    {
        var hasDuplicateScopeKeys = routeScopes.Select(x => x.Key).Distinct().Count() < routeScopes.Count();
        if (hasDuplicateScopeKeys)
            return Result.Failed(ResultFailureType.BadRequest, $"{nameof(RouteScope)} collection cannot have duplicate {nameof(RouteScope.Key)} values.");

        IEnumerable<Route[]> routesCollection = routeScopes.Select(x => x.Routes);

        foreach (var routes in routesCollection)
        {
            var hasDuplicateRouteKeys = routes.Select(x => x.Key).Distinct().Count() < routes.Length;
            if (hasDuplicateRouteKeys)
                return Result.Failed(ResultFailureType.BadRequest, $"{nameof(Route)} collection cannot have duplicate {nameof(Route.Key)} values under the same {nameof(RouteScope)}.");
        }

        return Result.Ok();
    }

    public INavigator? GetNavigator(string scopeKey)
    {
        _logger.LogInformation($"{nameof(GetNavigator)} | key: {{key}}", scopeKey);

        if (_navigatorScopesDict.TryGetValue(scopeKey, out Navigator? navigatorScope))
            return navigatorScope;

        if (!_routeScopesDict.TryGetValue(scopeKey, out RouteScope? routeScope))
            return null;

        navigatorScope = new Navigator(_serviceProvider.GetRequiredService<ILogger<Navigator>>(), routeScope);
        _navigatorScopesDict[scopeKey] = navigatorScope;

        return navigatorScope;
    }

    public INavigator GetRequiredNavigator(string scopeKey)
    {
        _logger.LogInformation($"{nameof(GetRequiredNavigator)} | key: {{key}}", scopeKey);
        return GetNavigator(scopeKey) ?? throw new ArgumentException($"Scope key, \"{scopeKey}\" is not defined.", nameof(scopeKey));
    }

    public IEnumerable<INavigator> GetAllNavigators()
    {
        _logger.LogInformation(nameof(GetAllNavigators));

        IEnumerable<INavigator> navigators = _routeScopesDict
            .Select(x => x.Key)
            .Select(GetNavigator)
            .Where(nav => nav != null)!;

        return navigators;
    }
}