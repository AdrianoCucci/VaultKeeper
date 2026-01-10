using Avalonia;
using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class ChangePasswordFormView : ViewBase<ChangePasswordFormViewModel>
{
    public static readonly RoutedEvent<FormActionEventArgs<ChangePasswordForm>> ActionInvokedEvent =
        RoutedEvent.Register<FormActionEventArgs<ChangePasswordForm>>(nameof(ActionInvoked), RoutingStrategies.Bubble, typeof(ChangePasswordFormView));

    public static readonly StyledProperty<bool> HideFormButtonsProperty = AvaloniaProperty.Register<ChangePasswordFormView, bool>(nameof(HideFormButtons));

    public event EventHandler<FormActionEventArgs<ChangePasswordForm>> ActionInvoked { add => AddHandler(ActionInvokedEvent, value); remove => RemoveHandler(ActionInvokedEvent, value); }

    public bool HideFormButtons { get => GetValue(HideFormButtonsProperty); set => SetValue(HideFormButtonsProperty, value); }

    public ChangePasswordFormView() => InitializeComponent();

    public void Cancel()
    {
        if (Model == null) return;

        RaiseEvent(new FormActionEventArgs<ChangePasswordForm>(ActionInvokedEvent, this)
        {
            Form = Model.Form,
            Action = FormAction.Cancelled
        });
    }

    public bool Submit()
    {
        bool isValid = Model?.Form.Validate() == true;

        if (isValid)
        {
            RaiseEvent(new FormActionEventArgs<ChangePasswordForm>(ActionInvokedEvent, this)
            {
                Form = Model!.Form,
                Action = FormAction.Submitted
            });
        }

        return isValid;
    }

    private void CancelButton_Clicked(object? sender, RoutedEventArgs e) => Cancel();

    private void ConfirmButton_Clicked(object? sender, RoutedEventArgs e) => Submit();
}