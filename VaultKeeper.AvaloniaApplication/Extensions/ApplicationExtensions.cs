using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace VaultKeeper.AvaloniaApplication.Extensions;

public static class ApplicationExtensions
{
    public static T? GetResourceOrDefault<T>(this Application application, object key, ThemeVariant? themeVariant = null, T? defaultValue = default)
    {
        if (application == null) return defaultValue;

        if (application.TryFindResource(key, themeVariant, out object? resource) && resource is T concreteResource)
            return concreteResource;

        return defaultValue;
    }
}
