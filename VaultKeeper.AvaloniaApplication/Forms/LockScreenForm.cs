using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VaultKeeper.AvaloniaApplication.Forms.Common;

namespace VaultKeeper.AvaloniaApplication.Forms;

public partial class LockScreenForm : Form
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Password is required.")]
    [CustomValidation(typeof(Form), nameof(ValidateExternalErrors))]
    private string? _passwordInput;

    protected override void OnExternalErrorsUpdated(Dictionary<string, string> externalErrors)
    {
        if (externalErrors.ContainsKey(nameof(PasswordInput)))
            ValidateProperty(PasswordInput, nameof(PasswordInput));
    }
}