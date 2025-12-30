using Avalonia.Interactivity;
using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public class VaultItemActionEventArgs(RoutedEvent routedEvent) : RoutedEventArgs(routedEvent)
{
    public required VaultItemViewModelBase ViewModel { get; init; }
    public required VaultItemAction Action { get; init; }

    [SetsRequiredMembers]
    public VaultItemActionEventArgs(RoutedEvent routedEvent, VaultItemActionEventArgs other) : this(routedEvent)
    {
        ViewModel = other.ViewModel;
        Action = other.Action;
    }
}
