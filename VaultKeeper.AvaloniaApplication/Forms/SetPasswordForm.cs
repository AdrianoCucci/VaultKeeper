using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VaultKeeper.AvaloniaApplication.Forms.Common;

namespace VaultKeeper.AvaloniaApplication.Forms;

public partial class SetPasswordForm : Form
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Password is required.")]
    private string? _password;

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Confirm Password is required.")]
    [MatchValue(nameof(Password), ErrorMessage = "Passwords do not match.")]
    private string? _confirmPassword;
}