using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemView : VaultItemViewBase<VaultItemViewModel>
{
    public VaultItemView() => InitializeComponent();

    protected override void OnPointerEntered(PointerEventArgs e) => UpdateModel(x => x.IsFocused = true);

    protected override void OnPointerExited(PointerEventArgs e) => UpdateModel(x => x.IsFocused = x.OptionsMenuOpened);

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

    private void VaultItem_ActionInvoked(object? sender, VaultItemActionEventArgs e) => RaiseEvent(e.Action);

    private void VaultItemFormView_FormActionInvoked(object? sender, VaultItemFormActionEventArgs e) => RaiseEvent(e.Action, e.Form);

    private void MenuFlyout_Opened(object? sender, EventArgs e) => UpdateModel(x => x.OptionsMenuOpened = true);

    private void MenuFlyout_Closed(object? sender, EventArgs e) => UpdateModel(x => x.OptionsMenuOpened = false);

    private void ActionCopyValue_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.CopyValue);

    private void ActionCopyName_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.CopyName);

    private void ActionEdit_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.Edit);

    private void ActionGroup_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.Group);

    private void ActionDelete_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemAction.Delete);
}