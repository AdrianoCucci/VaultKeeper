using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class LockScreenViewModel(ISecurityService securityService, ICache<UserData> userDataCache) : ViewModelBase
{
    private readonly ISecurityService _securityService = securityService;
    private readonly ICache<UserData> _userDataCache = userDataCache;

    public LockScreenForm Form { get; } = new();

    public bool SubmitForm()
    {
        Form.SubmissionError = null;
        if (!Form.Validate())
            return false;

        Result<UserData?> getUserData = _userDataCache.Get();
        if (!getUserData.IsSuccessful)
        {
            // TODO: Handle error.
            return false;
        }

        string inputPassword = Form.PasswordInput!;
        string? expectedPasswordHash = getUserData.Value?.MainPasswordHash;

        if (expectedPasswordHash == null)
        {
            // TODO: Handle invalid case.
            return false;
        }

        Result<bool> compareHashResult = _securityService.CompareHash(inputPassword, expectedPasswordHash);
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
