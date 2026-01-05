using Avalonia.Interactivity;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public class ConfirmPromptEventArgs(RoutedEvent routedEvent, object? source = null) : RoutedEventArgs(routedEvent, source)
{
    public required ConfirmPromptAction Action { get; init; }
    public required ConfirmPromptViewModel ViewModel { get; init; }
}