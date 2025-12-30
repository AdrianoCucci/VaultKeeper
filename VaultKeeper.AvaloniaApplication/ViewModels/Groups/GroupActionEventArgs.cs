using Avalonia.Interactivity;
using System.Diagnostics.CodeAnalysis;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public class GroupActionEventArgs(RoutedEvent routedEvent) : RoutedEventArgs(routedEvent)
{
    public required Group Group { get; init; }
    public required GroupAction Action { get; init; }

    [SetsRequiredMembers]
    public GroupActionEventArgs(RoutedEvent routedEvent, GroupActionEventArgs other) : this(routedEvent)
    {
        Group = other.Group;
        Action = other.Action;
    }
}
