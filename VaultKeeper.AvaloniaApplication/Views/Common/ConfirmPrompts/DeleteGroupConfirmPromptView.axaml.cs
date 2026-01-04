using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.ConfirmPrompts;
using VaultKeeper.Common.Models.Queries;

namespace VaultKeeper.AvaloniaApplication.Views.Common.ConfirmPrompts;

public partial class DeleteGroupConfirmPromptView : ConfirmPromptViewBase<DeleteGroupConfirmPromptViewModel>
{
    public DeleteGroupConfirmPromptView() => InitializeComponent();

    private void KeepChildrenButton_Click(object? sender, RoutedEventArgs e) => UpdateModel(x => x.CascadeDeleteMode = CascadeDeleteMode.OrphanChildren);

    private void DeleteChildrenButton_Click(object? sender, RoutedEventArgs e) => UpdateModel(x => x.CascadeDeleteMode = CascadeDeleteMode.DeleteChildren);

    private void CancelButton_Clicked(object? sender, RoutedEventArgs e) => RaiseEvent(ConfirmPromptAction.Cancelled);

    private void ConfirmButton_Clicked(object? sender, RoutedEventArgs e) => RaiseEvent(ConfirmPromptAction.Confirmed);
}