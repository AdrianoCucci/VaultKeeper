using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public abstract class ConfirmPromptViewBase<TModel> : PromptViewBase<TModel> where TModel : ConfirmPromptViewModel
{
    public static readonly RoutedEvent<ConfirmPromptEventArgs> ActionInvokedEvent =
        RoutedEvent.Register<ConfirmPromptEventArgs>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(ConfirmPromptViewBase<TModel>));

    public event EventHandler<ConfirmPromptEventArgs> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    protected void RaiseEvent(ConfirmPromptAction action)
    {
        if (Model == null) return;

        RaiseEvent(new RoutedEventArgs(AcknowledgedEvent, this));
        RaiseEvent(new ConfirmPromptEventArgs(ActionInvokedEvent, this)
        {
            Action = action,
            ViewModel = Model
        });
    }

    protected async Task InvokeActionAsync(ConfirmPromptAction action)
    {
        if (Model == null) return;

        await Model.InvokeActionAsync(action);
        RaiseEvent(action);
    }
}
