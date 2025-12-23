using Avalonia.Interactivity;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common;

public class FormEventArgs<T>(RoutedEvent routedEvent) : RoutedEventArgs(routedEvent) where T : class
{
    public required Form<T> Form { get; init; }
    public required FormAction Action { get; init; }
}
