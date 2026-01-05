using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views;

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

    private async void SearchBox_Debounce(object? sender, TextInputEventArgs e) => await LoadDataAsync();

    private void SortButton_Click(object? sender, RoutedEventArgs e) => Model?.ToggleSortDirection();

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

    private void OverlayPanel_OverlayClosed(object? sender, RoutedEventArgs e) => Model?.HideOverlay();

    private async void ConfirmPromptView_ActionInvoked(object? sender, ConfirmPromptEventArgs e) => await e.ViewModel.InvokeActionAsync(e.Action);

    private void PromptView_Acknowledged(object? sender, RoutedEventArgs e) => Model?.HideOverlay();
}