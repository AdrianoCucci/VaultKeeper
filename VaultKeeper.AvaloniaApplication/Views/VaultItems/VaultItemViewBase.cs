using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public abstract class VaultItemViewBase<T> : ViewBase<T> where T : VaultItemViewModelBase
{
    public static readonly RoutedEvent<VaultItemActionEventArgs> ActionInvokedEvent = RoutedEvent.Register<VaultItemActionEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(VaultItemViewBase<>));

    public event EventHandler<VaultItemActionEventArgs> ActionInvoked
    {
        add => AddHandler(ActionInvokedEvent, value);
        remove => RemoveHandler(ActionInvokedEvent, value);
    }

    protected void RaiseEvent(VaultItemAction action) => RaiseEvent(new VaultItemActionEventArgs(action, ActionInvokedEvent));
}