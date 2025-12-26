using Avalonia.Interactivity;

namespace VaultKeeper.AvaloniaApplication.Models.Common;

public class RoutedEventArgs<T>(T value) : RoutedEventArgs
{
    public T Value { get; } = value;
}
