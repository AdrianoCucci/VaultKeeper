using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.AvaloniaApplication.Views.Groups;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class GroupedVaultItemsView : ViewBase<GroupedVaultItemsViewModel>
{
    public static readonly RoutedEvent<VaultItemActionEventArgs> ItemActionInvokedEvent =
        RoutedEvent.Register<VaultItemActionEventArgs>(nameof(ItemActionInvoked), RoutingStrategies.Bubble, typeof(GroupedVaultItemsView));

    public static readonly RoutedEvent<VaultItemFormActionEventArgs> ItemFormActionInvokedEvent =
        RoutedEvent.Register<VaultItemFormActionEventArgs>(nameof(ItemFormActionInvoked), RoutingStrategies.Bubble, typeof(GroupedVaultItemsView));

    public static readonly RoutedEvent<GroupActionEventArgs> GroupActionInvokedEvent =
        RoutedEvent.Register<GroupActionEventArgs>(nameof(GroupActionInvoked), RoutingStrategies.Bubble, typeof(GroupView));

    public event EventHandler<VaultItemActionEventArgs> ItemActionInvoked
    {
        add => AddHandler(ItemActionInvokedEvent, value);
        remove => RemoveHandler(ItemActionInvokedEvent, value);
    }

    public event EventHandler<VaultItemFormActionEventArgs> ItemFormActionInvoked
    {
        add => AddHandler(ItemFormActionInvokedEvent, value);
        remove => RemoveHandler(ItemFormActionInvokedEvent, value);
    }

    public event EventHandler<GroupActionEventArgs> GroupActionInvoked
    {
        add => AddHandler(GroupActionInvokedEvent, value);
        remove => RemoveHandler(GroupActionInvokedEvent, value);
    }

    public GroupedVaultItemsView() => InitializeComponent();

    private void VaultItemView_ActionInvoked(object? sender, VaultItemActionEventArgs e) =>
        RaiseEvent(new VaultItemActionEventArgs(ItemActionInvokedEvent, e));

    private void VaultItemFormView_FormActionInvoked(object? sender, VaultItemFormActionEventArgs e) =>
        RaiseEvent(new VaultItemFormActionEventArgs(ItemFormActionInvokedEvent, e));

    private void GroupView_ActionInvoked(object? sender, GroupActionEventArgs e) =>
        RaiseEvent(new GroupActionEventArgs(GroupActionInvokedEvent, e));
}