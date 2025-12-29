using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.Views.Groups;

public partial class GroupFormView : ViewBase<GroupFormViewModel>
{
    public static readonly RoutedEvent<GroupActionEventArgs> ActionInvokedEvent =
        RoutedEvent.Register<GroupActionEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(GroupFormView));

    public event EventHandler<GroupActionEventArgs> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    public GroupFormView() => InitializeComponent();

    private void RaiseEvent(GroupAction action, Group? group = null)
    {
        if (Model == null) return;

        RaiseEvent(new GroupActionEventArgs(ActionInvokedEvent)
        {
            Action = action,
            Group = group ?? Model.Model
        });
    }

    private void FormButtons_Cancelled(object? sender, RoutedEventArgs e) => RaiseEvent(GroupAction.CancelEdit);

    private void FormButtons_Submitted(object? sender, RoutedEventArgs e)
    {
        if (Model?.Form?.Validate() == true)
            RaiseEvent(GroupAction.ConfirmEdit, Model.Form.GetModel());
    }
}