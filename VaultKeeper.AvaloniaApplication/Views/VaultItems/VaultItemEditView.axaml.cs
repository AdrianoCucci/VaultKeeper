using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemEditView : VaultItemViewBase<VaultItemEditViewModel>
{
    public VaultItemEditView() => InitializeComponent();

    private void ActionToggleRevealValue_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.ToggleRevealValue);

    private void ActionEditCancel_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.EditCancel);

    private void ActionEditSave_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.EditSave);
}