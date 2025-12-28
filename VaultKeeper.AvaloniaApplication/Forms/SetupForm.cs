using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VaultKeeper.AvaloniaApplication.Forms.Common;

namespace VaultKeeper.AvaloniaApplication.Forms;

public partial class SetupForm : Form
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Password is required.")]
    private string? _passwordInput;

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Confirm Password is required.")]
    [MatchValue(nameof(PasswordInput), ErrorMessage = "Passwords do not match.")]
    private string? _confirmPasswordInput;
}