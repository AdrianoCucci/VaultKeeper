using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VaultKeeper.AvaloniaApplication.Forms.Common;

namespace VaultKeeper.AvaloniaApplication.Forms;

public partial class LockScreenForm : Form
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Password is required.")]
    private string? _passwordInput;

    [ObservableProperty]
    private string? _submissionError;
}