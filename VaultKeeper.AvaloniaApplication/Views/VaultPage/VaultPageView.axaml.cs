using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultPage;

namespace VaultKeeper.AvaloniaApplication.Views.VaultPage;

public partial class VaultPageView : ViewBase<VaultPageViewModel>
{
    public VaultPageView() => InitializeComponent();

    protected override async void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        await LoadDataAsync();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Model?.HideAllForms();
    }

    private async Task LoadDataAsync()
    {
        if (Model != null)
            await Model.LoadDataAsync();
    }

    private void Root_LayoutUpdated(object? sender, EventArgs e) => UpdateModel(x => x.ModalBounds = Root.Bounds.Deflate(20));

    private void ButtonNewKey_Click(object? sender, RoutedEventArgs e) => Model?.ShowVaultItemCreateForm();

    private void ButtonImportKeys_Click(object? sender, RoutedEventArgs e) => Model?.ShowImportItemsOverlay();

    private async void Toolbar_ActionInvoked(object? sender, VaultPageToolbarEventArgs e)
    {
        if (Model != null)
            await Model.HandleToolbarActionAsync(e);
    }

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

    private void OverlayPanel_OverlayClosed(object? sender, RoutedEventArgs e) => Model?.HideOverlay();

    private void Modal_BackdropPressed(object? sender, RoutedEventArgs e) => Model?.HideOverlay();

    private async void VaultItemImportView_ProcessSucceeded(object? sender, RoutedEventArgs e)
    {
        if (Model is not VaultPageViewModel model) return;

        await model.LoadDataAsync();
        model.HideOverlay();
    }

    private void VirtualizingStackPanel_KeyDown(object? sender, KeyEventArgs e) => PART_PageLayoutPanel.MoveScrollViewerPosition(e);
}