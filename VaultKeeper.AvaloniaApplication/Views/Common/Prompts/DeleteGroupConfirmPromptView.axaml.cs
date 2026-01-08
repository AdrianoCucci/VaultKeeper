using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;
using VaultKeeper.Common.Models.Queries;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public partial class DeleteGroupConfirmPromptView : ConfirmPromptViewBase<DeleteGroupConfirmPromptViewModel>
{
    public DeleteGroupConfirmPromptView() => InitializeComponent();

    private void KeepChildrenButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model is DeleteGroupConfirmPromptViewModel deleteGroupVM)
            deleteGroupVM.CascadeDeleteMode = CascadeDeleteMode.OrphanChildren;
    }

    private void DeleteChildrenButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model is DeleteGroupConfirmPromptViewModel deleteGroupVM)
            deleteGroupVM.CascadeDeleteMode = CascadeDeleteMode.DeleteChildren;
    }

    private async void CancelButton_Clicked(object? sender, RoutedEventArgs e) => await InvokeActionAsync(ConfirmPromptAction.Cancelled);

    private async void ConfirmButton_Clicked(object? sender, RoutedEventArgs e) => await InvokeActionAsync(ConfirmPromptAction.Confirmed);
}