using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;

namespace VaultKeeper.AvaloniaApplication.Views.Groups;

public partial class GroupView : ViewBase<GroupViewModel>
{
    public static readonly RoutedEvent<GroupActionEventArgs> ActionInvokedEvent =
        RoutedEvent.Register<GroupActionEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(GroupView));

    public event EventHandler<GroupActionEventArgs> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    public GroupView() => InitializeComponent();

    private void RaiseEvent(GroupAction action)
    {
        if (Model == null) return;

        RaiseEvent(new GroupActionEventArgs(ActionInvokedEvent)
        {
            Action = action,
            Group = Model.Model,
            Source = this
        });
    }

    private void ActionEdit_Click(object? sender, RoutedEventArgs e) => RaiseEvent(GroupAction.Edit);

    private void ActionAddKey_Click(object? sender, RoutedEventArgs e) => RaiseEvent(GroupAction.AddItem);

    private void ActionDelete_Click(object? sender, RoutedEventArgs e) => RaiseEvent(GroupAction.Delete);
}