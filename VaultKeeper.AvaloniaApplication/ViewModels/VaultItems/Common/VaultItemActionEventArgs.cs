using Avalonia.Interactivity;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public class VaultItemActionEventArgs(RoutedEvent routedEvent, VaultItemAction action) : RoutedEventArgs(routedEvent)
{
    public VaultItemAction Action { get; } = action;
}
