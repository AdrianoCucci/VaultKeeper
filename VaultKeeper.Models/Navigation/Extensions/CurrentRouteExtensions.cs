namespace VaultKeeper.Models.Navigation.Extensions;

public static class CurrentRouteExtensions
{
    public static object? GetParamOrDefault(this CurrentRoute route, string paramName, object? defaultValue = default)
    {
        if (route.Params.TryGetValue(paramName, out object? value))
            return value;

        return defaultValue;
    }

    public static T? GetParamOrDefault<T>(this CurrentRoute route, string paramName, T? defaultValue = default)
    {
        if (GetParamOrDefault(route, paramName) is T concreteValue)
            return concreteValue;

        return defaultValue;
    }
}
