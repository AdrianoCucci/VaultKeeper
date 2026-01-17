using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public partial class PromptView : PromptViewBase<PromptViewModel>
{
    public PromptView() => InitializeComponent();

    private void Prompt_Acknowledged(object? sender, RoutedEventArgs e)
    {
        Model?.Acknowledge();
        RaiseEvent(new(AcknowledgedEvent, this));
    }
}