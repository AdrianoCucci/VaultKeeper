using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Forms;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public class VaultItemFormActionEventArgs(RoutedEvent routedEvent) : RoutedEventArgs(routedEvent)
{
    public required VaultItemFormViewModel ViewModel { get; init; }
    public required Form<VaultItem> Form { get; init; }
    public required VaultItemFormAction Action { get; init; }
}
