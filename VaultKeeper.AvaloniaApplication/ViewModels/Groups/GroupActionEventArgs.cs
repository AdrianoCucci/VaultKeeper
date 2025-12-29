using Avalonia.Interactivity;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public class GroupActionEventArgs(RoutedEvent routedEvent) : RoutedEventArgs(routedEvent)
{
    public required Group Group { get; init; }
    public required GroupAction Action { get; init; }
}
