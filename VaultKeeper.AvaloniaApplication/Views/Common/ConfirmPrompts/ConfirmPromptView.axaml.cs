using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.ConfirmPrompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.ConfirmPrompts;

public partial class ConfirmPromptView : ConfirmPromptViewBase<ConfirmPromptViewModel>
{
    public ConfirmPromptView() => InitializeComponent();

    private void CancelButton_Clicked(object? sender, RoutedEventArgs e) => RaiseEvent(ConfirmPromptAction.Cancelled);

    private void ConfirmButton_Clicked(object? sender, RoutedEventArgs e) => RaiseEvent(ConfirmPromptAction.Confirmed);
}