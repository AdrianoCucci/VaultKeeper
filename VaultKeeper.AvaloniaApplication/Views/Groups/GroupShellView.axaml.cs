using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;

namespace VaultKeeper.AvaloniaApplication.Views.Groups;

public partial class GroupShellView : ViewBase<GroupShellViewModel>
{
    public static readonly RoutedEvent<GroupActionEventArgs> ActionInvokedEvent =
        RoutedEvent.Register<GroupActionEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(GroupShellView));

    public event EventHandler<GroupActionEventArgs> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    public GroupShellView() => InitializeComponent();

    private void Group_ActionInvoked(object? sender, GroupActionEventArgs e) => RaiseEvent(new GroupActionEventArgs(ActionInvokedEvent, e));
}