using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public abstract class PromptViewBase<TModel> : ViewBase<TModel> where TModel : PromptViewModel
{
    public static readonly RoutedEvent<RoutedEventArgs> AcknowledgedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(Acknowledged), RoutingStrategies.Bubble, typeof(PromptViewBase<TModel>));

    public event EventHandler<RoutedEventArgs> Acknowledged { add => AddHandler(AcknowledgedEvent, value); remove => RemoveHandler(AcknowledgedEvent, value); }
}
