using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class VaultPageView : ViewBase<VaultPageViewModel>
{
    public VaultPageView() => InitializeComponent();

    protected override async void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (Model != null)
            await Model.LoadDataAsync();

        base.OnApplyTemplate(e);
    }

    private void ButtonNew_Click(object? sender, RoutedEventArgs e) => Model?.ShowVaultItemCreateForm();

    private void ButtonCloseSidePane_Click(object? sender, RoutedEventArgs e) => Model?.HideVaultItemCreateForm();

    private async void VaultItem_ActionInvoked(object? sender, VaultItemActionEventArgs e)
    {
        if (Model != null)
            await Model.HandleItemActionAsync(e);
    }

    private async void VaultItem_FormActionInvoked(object? sender, VaultItemFormActionEventArgs e)
    {
        if (Model != null)
            await Model.HandleItemFormActionAsync(e);
    }

    private async void Group_ActionInvoked(object? sender, GroupActionEventArgs e)
    {
        if (Model != null)
            await Model.HandleGroupActionAsnc(e);
    }

    private async void SplitView_PaneClosed(object? sender, RoutedEventArgs e)
    {
        await Task.Delay(250);
        UpdateModel(x => x.SidePaneContent = null);
    }
}