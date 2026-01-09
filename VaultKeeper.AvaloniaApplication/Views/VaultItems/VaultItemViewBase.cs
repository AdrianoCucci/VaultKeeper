using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public abstract class VaultItemViewBase<TModel> : ViewBase<TModel> where TModel : ViewModelBase
{
    public static readonly RoutedEvent<VaultItemActionEventArgs> ActionInvokedEvent =
        RoutedEvent.Register<VaultItemActionEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(VaultItemViewBase<TModel>));

    public static readonly RoutedEvent<VaultItemFormActionEventArgs> FormActionInvokedEvent =
        RoutedEvent.Register<VaultItemFormActionEventArgs>(nameof(FormActionInvoked), RoutingStrategies.Bubble, typeof(VaultItemViewBase<TModel>));

    public event EventHandler<VaultItemActionEventArgs> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    public event EventHandler<VaultItemFormActionEventArgs> FormActionInvoked { add => AddHandler(FormActionInvokedEvent, value); remove => RemoveHandler(FormActionInvokedEvent, value); }

    protected void RaiseEvent(VaultItemAction action, VaultItemViewModelBase viewModel)
    {
        if (Model == null) return;

        RaiseEvent(new VaultItemActionEventArgs(ActionInvokedEvent)
        {
            Action = action,
            ViewModel = viewModel
        });
    }

    protected void RaiseEvent(VaultItemFormAction action, VaultItemFormViewModel viewModel)
    {
        RaiseEvent(new VaultItemFormActionEventArgs(FormActionInvokedEvent)
        {
            ViewModel = viewModel,
            Action = action,
            Form = viewModel.Form
        });
    }
}