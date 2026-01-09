using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemShellView : VaultItemViewBase<VaultItemShellViewModel>
{
    public VaultItemShellView() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Model!.PropertyChanged += Model_PropertyChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (Model != null)
            Model.PropertyChanged -= Model_PropertyChanged;
    }

    protected override void OnPointerEntered(PointerEventArgs e) => UpdateModel(x => x.IsFocused = true);

    protected override void OnPointerExited(PointerEventArgs e) =>
        UpdateModel(x => x.IsFocused = x.Content is VaultItemViewModel normalVM && normalVM.OptionsMenuOpened);

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        UpdateModel(x => x.IsFocused = true);
        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateModel(x => x.IsFocused = false);
        base.OnLostFocus(e);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Model?.Content == null) return;

        if (e.PropertyName == nameof(Model.IsSelected))
        {
            VaultItemAction action = Model.IsSelected == true ? VaultItemAction.Select : VaultItemAction.Deselect;
            RaiseEvent(action, Model.Content);
        }
    }

    private void VaultItem_ActionInvoked(object? sender, VaultItemActionEventArgs e) => RaiseEvent(e.Action, e.ViewModel);

    private void VaultItem_FormActionInvoked(object? sender, VaultItemFormActionEventArgs e) => RaiseEvent(e.Action, e.ViewModel);
}