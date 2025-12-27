using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels.LockScreen;

public partial class LockScreenViewModel(ISecurityService securityService, ICache<UserData> userDataCache) : ViewModelBase
{
    public LockScreenForm Form { get; } = new();

    public bool SubmitForm()
    {
        Form.SubmissionError = null;
        if (!Form.Validate())
            return false;

        Result<UserData?> getUserData = userDataCache.Get();
        if (!getUserData.IsSuccessful)
        {
            // TODO: Handle error.
            return false;
        }

        string inputPassword = Form.PasswordInput!;
        string? expectedPasswordHash = getUserData.Value!.MainPasswordHash;

        if (expectedPasswordHash == null)
        {
            // TODO: Handle invalid case.
            return false;
        }

        Result<bool> compareHashResult = securityService.CompareHash(inputPassword, expectedPasswordHash);
        if (!compareHashResult.IsSuccessful)
        {
            // TODO: Handle error.
            return false;
        }

        bool isMatch = compareHashResult.Value;
        if (!isMatch)
            Form.SubmissionError = "Password is invalid.";

        return isMatch;
    }
}
