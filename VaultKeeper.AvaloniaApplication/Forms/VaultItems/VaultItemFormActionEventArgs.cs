using Avalonia.Interactivity;
using System.Diagnostics.CodeAnalysis;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Forms.VaultItems;

public class VaultItemFormActionEventArgs(RoutedEvent routedEvent) : RoutedEventArgs(routedEvent)
{
    public required VaultItemFormViewModel ViewModel { get; init; }
    public required VaultItemForm Form { get; init; }
    public required VaultItemFormAction Action { get; init; }

    [SetsRequiredMembers]
    public VaultItemFormActionEventArgs(RoutedEvent routedEvent, VaultItemFormActionEventArgs other) : this(routedEvent)
    {
        ViewModel = other.ViewModel;
        Form = other.Form;
        Action = other.Action;
    }
}
