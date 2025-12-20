using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.AvaloniaApplication.Views.VaultItems;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class VaultPageView : ViewBase<VaultPageViewModel>
{
    public VaultPageView() => InitializeComponent();

    private async void VaultItemView_ActionInvoked(object? sender, VaultItemActionEventArgs e)
    {
        if (Model == null) return;
        if (e.Source is not VaultItemView itemView) return;
        if (itemView.Model is not VaultItemViewModel itemVM) return;

        await Model.HandleVaultItemEventActionAsync(itemVM, e.Action);
    }
}