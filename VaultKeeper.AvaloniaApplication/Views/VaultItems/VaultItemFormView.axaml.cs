using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.Extensions;
using VaultKeeper.AvaloniaApplication.Forms.VaultItems;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemFormView : VaultItemViewBase<VaultItemFormViewModel>
{
    public VaultItemFormView() => InitializeComponent();

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InputName.FocusEnd();
    }

    private void RaiseEvent(VaultItemFormAction action)
    {
        if (Model == null) return;
        RaiseEvent(action, Model.Form);
    }

    private void ActionToggleRevealValue_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemFormAction.ToggleRevealValue);

    private void FormButtons_Cancelled(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemFormAction.Cancel);

    private void FormButtons_Submitted(object? sender, RoutedEventArgs e)
    {
        Model?.Form?.Validate();
        RaiseEvent(VaultItemFormAction.Submit);
    }

    private void Root_LayoutUpdated(object? sender, EventArgs e) => UpdateModel(x => x.UseVerticalLayout = Root.Bounds.Width < 800);
}