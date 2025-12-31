using Avalonia;
using Avalonia.Controls;

namespace VaultKeeper.AvaloniaApplication.Extensions;

public static class ApplicationExtensions
{
    public static T? GetResourceOrDefault<T>(this Application application, object key, T? defaultValue = default)
    {
        if (application == null) return defaultValue;

        if (application.TryFindResource(key, out object? resource) && resource is T concreteResource)
            return concreteResource;

        return defaultValue;
    }
}
