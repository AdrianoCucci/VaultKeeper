using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VaultKeeper.AvaloniaApplication.Forms;

public partial class ChangePasswordForm : SetPasswordForm
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Current Password is required.")]
    private string? _currentPassword;
}