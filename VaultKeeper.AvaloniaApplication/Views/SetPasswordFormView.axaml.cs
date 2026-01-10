using Avalonia.Interactivity;
using System;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class SetPasswordFormView : ViewBase<SetPasswordFormViewModel>
{
    public static readonly RoutedEvent<FormActionEventArgs<SetPasswordForm>> SubmittedEvent =
        RoutedEvent.Register<FormActionEventArgs<SetPasswordForm>>(nameof(Submitted), RoutingStrategies.Bubble, typeof(SetPasswordFormView));

    public event EventHandler<FormActionEventArgs<SetPasswordForm>> Submitted { add => AddHandler(SubmittedEvent, value); remove => RemoveHandler(SubmittedEvent, value); }

    public SetPasswordFormView() => InitializeComponent();

    private void SubmitButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Model?.Form.Validate() == true)
        {
            RaiseEvent(new FormActionEventArgs<SetPasswordForm>(SubmittedEvent, this)
            {
                Form = Model.Form,
                Action = FormAction.Submitted
            });
        }
    }
}