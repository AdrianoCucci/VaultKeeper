using Avalonia.Interactivity;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultPage;

public class VaultPageToolbarEventArgs(RoutedEvent routedEvent, object? source = null) : RoutedEventArgs(routedEvent, source)
{
    public required VaultPageToolbarViewModel ViewModel { get; init; }
    public required VaultPageToolbarAction Action { get; init; }
}
