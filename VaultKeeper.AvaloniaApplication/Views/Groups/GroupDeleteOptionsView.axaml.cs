using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.Common.Models.Queries;

namespace VaultKeeper.AvaloniaApplication.Views.Groups;

public partial class GroupDeleteOptionsView : ViewBase<GroupDeleteOptionsViewModel>
{
    public GroupDeleteOptionsView() => InitializeComponent();

    private void KeepChildrenButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model is GroupDeleteOptionsViewModel deleteGroupVM)
            deleteGroupVM.CascadeDeleteMode = CascadeDeleteMode.OrphanChildren;
    }

    private void DeleteChildrenButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model is GroupDeleteOptionsViewModel deleteGroupVM)
            deleteGroupVM.CascadeDeleteMode = CascadeDeleteMode.DeleteChildren;
    }
}