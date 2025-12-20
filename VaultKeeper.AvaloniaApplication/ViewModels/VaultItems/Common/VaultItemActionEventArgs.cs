using Avalonia.Interactivity;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public class VaultItemActionEventArgs(VaultItemAction action, RoutedEvent routedEvent) : RoutedEventArgs(routedEvent)
{
    public VaultItemAction Action { get; } = action;
}
