using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemReadOnlyView : VaultItemViewBase<VaultItemReadOnlyViewModel>
{
    public VaultItemReadOnlyView() => InitializeComponent();

    private void ActionCopyValue_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.CopyValue);

    private void ActionCopyName_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.CopyName);

    private void ActionEdit_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.Edit);

    private void ActionGroup_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.Group);

    private void ActionDelete_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.Delete);
}