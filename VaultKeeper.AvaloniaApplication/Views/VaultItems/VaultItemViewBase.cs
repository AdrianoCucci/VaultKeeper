using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public abstract class VaultItemViewBase<T> : ViewBase<T> where T : VaultItemViewModelBase
{
    public static readonly RoutedEvent<VaultItemActionEventArgs> ActionInvokedEvent =
        RoutedEvent.Register<VaultItemActionEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(VaultItemViewBase<>));

    public static readonly RoutedEvent<VaultItemFormActionEventArgs> FormActionInvokedEvent =
        RoutedEvent.Register<VaultItemFormActionEventArgs>(nameof(FormActionInvoked), RoutingStrategies.Bubble, typeof(VaultItemViewBase<>));

    public event EventHandler<VaultItemActionEventArgs> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    public event EventHandler<VaultItemFormActionEventArgs> FormActionInvoked { add => AddHandler(FormActionInvokedEvent, value); remove => RemoveHandler(FormActionInvokedEvent, value); }

    protected void RaiseEvent(VaultItemAction action) => RaiseEvent(new VaultItemActionEventArgs(ActionInvokedEvent, action));

    protected void RaiseEvent(VaultItemFormAction action, Form<VaultItem> form)
    {
        if (Model is not VaultItemFormViewModel formVM) return;

        RaiseEvent(new VaultItemFormActionEventArgs(FormActionInvokedEvent)
        {
            ViewModel = formVM,
            Action = action,
            Form = form
        });
    }
}