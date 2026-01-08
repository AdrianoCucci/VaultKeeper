using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public partial class GroupItemsConfirmPromptView : ConfirmPromptViewBase<GroupItemsConfirmPromptViewModel>
{
    public GroupItemsConfirmPromptView() => InitializeComponent();

    private async void CancelButton_Clicked(object? sender, RoutedEventArgs e) => await InvokeActionAsync(ConfirmPromptAction.Cancelled);

    private async void ConfirmButton_Clicked(object? sender, RoutedEventArgs e) => await InvokeActionAsync(ConfirmPromptAction.Confirmed);
}