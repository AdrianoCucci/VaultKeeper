using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemView : VaultItemViewBase<VaultItemViewModel>
{
    public VaultItemView() => InitializeComponent();

    private void VaultItem_ActionInvoked(object? sender, VaultItemActionEventArgs e) => RaiseEvent(e.Action);

    private void VaultItemFormView_FormActionInvoked(object? sender, VaultItemFormActionEventArgs e) => RaiseEvent(e.Action, e.Form);
}