using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public partial class ChangePasswordConfirmPromptView : ConfirmPromptViewBase<ChangePasswordConfirmPromptViewModel>
{
    public ChangePasswordConfirmPromptView() => InitializeComponent();

    private async void CancelButton_Clicked(object? sender, RoutedEventArgs e)
    {
        PART_FormView.Cancel();
        await InvokeActionAsync(ConfirmPromptAction.Cancelled);
    }

    private async void ConfirmButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (PART_FormView.Submit())
            await InvokeActionAsync(ConfirmPromptAction.Confirmed);
    }
}