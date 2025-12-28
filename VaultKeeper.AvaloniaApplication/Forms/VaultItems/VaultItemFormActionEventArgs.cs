using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.Forms.VaultItems;

public class VaultItemFormActionEventArgs(RoutedEvent routedEvent) : RoutedEventArgs(routedEvent)
{
    public required VaultItemFormViewModel ViewModel { get; init; }
    public required Form<VaultItem> Form { get; init; }
    public required VaultItemFormAction Action { get; init; }
}
