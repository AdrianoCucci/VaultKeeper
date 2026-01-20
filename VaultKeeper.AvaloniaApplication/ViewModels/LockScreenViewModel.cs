using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Errors;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class LockScreenPageViewModel(IAppSessionService appSessionService, IErrorReportingService errorReportingService) : ViewModelBase
{
    public LockScreenForm Form { get; } = new();

    public void Initialize()
    {
        Form.PasswordInput = null;
        Form.ClearErrors();
    }

    public async Task<bool> SubmitFormAsync()
    {
        if (!Form.Validate())
            return false;

        Result<bool> loginResult = await appSessionService.TryLoginAsync(Form.PasswordInput!);
        if (!loginResult.IsSuccessful)
        {
            errorReportingService.ReportError(new()
            {
                Header = "Login Failure",
                Message = $"({loginResult.FailureType}) - {loginResult.Message}",
                Exception = loginResult.Exception,
                Severity = ErrorSeverity.High
            });

            return false;
        }

        if (!loginResult.Value)
            Form.SetExternalError(nameof(Form.PasswordInput), "Password is invalid.");

        return loginResult.Value;
    }
}
