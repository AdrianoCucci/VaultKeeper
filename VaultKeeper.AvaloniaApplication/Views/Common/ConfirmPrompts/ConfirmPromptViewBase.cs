using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.ConfirmPrompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.ConfirmPrompts;

public abstract class ConfirmPromptViewBase<TModel> : ViewBase<TModel> where TModel : ConfirmPromptViewModel
{
    public static readonly RoutedEvent<ConfirmPromptEventArgs> ActionInvokedEvent =
        RoutedEvent.Register<ConfirmPromptEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(ConfirmPromptView));

    public event EventHandler<ConfirmPromptEventArgs> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    protected void RaiseEvent(ConfirmPromptAction action)
    {
        if (Model == null) return;

        RaiseEvent(new ConfirmPromptEventArgs(ActionInvokedEvent, this)
        {
            Action = action,
            ViewModel = Model
        });
    }
}
