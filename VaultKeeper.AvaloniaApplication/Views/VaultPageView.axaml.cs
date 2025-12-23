using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.AvaloniaApplication.Views.VaultItems;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class VaultPageView : ViewBase<VaultPageViewModel>
{
    public VaultPageView() => InitializeComponent();

    private void ButtonNew_Click(object? sender, RoutedEventArgs e) => Model?.ShowVaultItemCreateForm();

    private void ButtonCloseSidePane_Click(object? sender, RoutedEventArgs e) => Model?.HideVaultItemCreateForm();

    private async void VaultItemView_ActionInvoked(object? sender, VaultItemActionEventArgs e)
    {
        if (Model == null) return;
        if (e.Source is not VaultItemView itemView) return;
        if (itemView.Model is not VaultItemViewModel itemVM) return;

        await Model.HandleItemActionAsync(itemVM, e.Action);
    }

    private async void VaultItemFormView_FormActionInvoked(object? sender, VaultItemFormActionEventArgs e)
    {
        if (Model != null)
            await Model.HandleItemFormEventAsync(e);
    }
}