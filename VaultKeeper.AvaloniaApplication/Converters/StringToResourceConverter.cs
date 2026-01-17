using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace VaultKeeper.AvaloniaApplication.Converters;

public class StringToResourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string resourceKey && Application.Current?.TryFindResource(resourceKey, out object? resource) == true)
            return resource;

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
