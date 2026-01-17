using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.Extensions;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.Views.VaultItems;

public partial class VaultItemFormView : VaultItemViewBase<VaultItemFormViewModel>
{
    public VaultItemFormView() => InitializeComponent();

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InputName.FocusEnd();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Enter:
                Submit();
                break;
            case Key.Escape:
                Cancel();
                break;
        }
    }

    private void RaiseEvent(VaultItemFormAction action)
    {
        if (Model == null) return;
        RaiseEvent(action, Model);
    }

    public bool Submit()
    {
        bool isValid = Model?.Form?.Validate() == true;

        if (isValid)
            RaiseEvent(VaultItemFormAction.Submit);

        return isValid;
    }

    public void Cancel() => RaiseEvent(VaultItemFormAction.Cancel);

    private void ActionToggleRevealValue_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemFormAction.ToggleRevealValue);

    private void GenerateValueButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(VaultItemFormAction.GenerateValue);

    private void FormButtons_Cancelled(object? sender, RoutedEventArgs e) => Cancel();

    private void FormButtons_Submitted(object? sender, RoutedEventArgs e) => Submit();

    private void Root_LayoutUpdated(object? sender, EventArgs e) => UpdateModel(x => x.UseVerticalLayout = Root.Bounds.Width < 800);
}