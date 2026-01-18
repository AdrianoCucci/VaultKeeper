using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public class VaultItemActionEventArgs(RoutedEvent routedEvent, object? source = null) : RoutedEventArgs(routedEvent, source)
{
    public required VaultItemViewModelBase ViewModel { get; init; }
    public required VaultItemAction Action { get; init; }
    public KeyModifiers KeyModifiers { get; init; } = KeyModifiers.None;

    [SetsRequiredMembers]
    public VaultItemActionEventArgs(RoutedEvent routedEvent, VaultItemActionEventArgs other, object? source = null) : this(routedEvent, source ?? other.Source)
    {
        ViewModel = other.ViewModel;
        Action = other.Action;
        KeyModifiers = other.KeyModifiers;
    }
}