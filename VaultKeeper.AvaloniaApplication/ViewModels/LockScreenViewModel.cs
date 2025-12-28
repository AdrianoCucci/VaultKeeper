using System;
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

        UserData userData = _userDataCache.Get() ?? throw new InvalidOperationException($"{nameof(LockScreenViewModel)}: {nameof(UserData)} not set in cache.");
        string expectedPasswordHash = userData.MainPasswordHash ?? throw new InvalidOperationException($"{nameof(LockScreenViewModel)}: cached {nameof(UserData)} state is missing password hash.");
        string inputPassword = Form.PasswordInput!;

        Result<bool> compareHashResult = _securityService.CompareHash(inputPassword, expectedPasswordHash);
        if (!compareHashResult.IsSuccessful)
            throw new Exception($"{nameof(LockScreenViewModel)}: Failed to verify password - {compareHashResult.Message}", compareHashResult.Exception);

        bool isMatch = compareHashResult.Value;
        if (!isMatch)
            Form.SubmissionError = "Password is invalid.";

        return isMatch;
    }
}
