using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.Common.Results;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class LockScreenPageViewModel(IAppSessionService appSessionService) : ViewModelBase
{
    public LockScreenForm Form { get; } = new();

    public void Initialize()
    {
        Form.PasswordInput = null;
        Form.ClearErrors();
    }

    public async Task<bool> SubmitFormAsync()
    {
        Form.SubmissionError = null;
        if (!Form.Validate())
            return false;

        Result<bool> loginResult = await appSessionService.TryLoginAsync(Form.PasswordInput!);
        if (!loginResult.IsSuccessful)
        {
            // TODO: handle error.
            if (loginResult.FailureType == ResultFailureType.Conflict)
            {
                // TODO: main password not set - inform user.
            }

            return false;
        }

        if (!loginResult.Value)
            Form.SubmissionError = "Password is invalid.";

        return loginResult.Value;
    }
}
