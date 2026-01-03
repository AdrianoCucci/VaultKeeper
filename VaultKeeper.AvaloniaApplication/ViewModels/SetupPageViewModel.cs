using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public class SetupPageViewModel(IAppDataService appDataService, ISecurityService securityService, IBackupService backupService) : ViewModelBase
{
    public SetupForm Form { get; } = new();

    public async Task<bool> SubmitFormAsync()
    {
        if (!Form.Validate()) return false;

        Result<string> hashPasswordResult = securityService.CreateHash(Form.PasswordInput!);
        if (!hashPasswordResult.IsSuccessful)
            throw new Exception($"{nameof(SetupPageViewModel)}: Failed to submit form: {hashPasswordResult.Message}", hashPasswordResult.Exception);

        UserData userData = new()
        {
            UserId = Guid.NewGuid(),
            MainPasswordHash = hashPasswordResult.Value!
        };

        Result<SavedData<UserData>?> saveUserDataResult = await appDataService.SaveUserDataAsync(userData, updateCaches: true);
        if (!saveUserDataResult.IsSuccessful)
            throw new Exception($"{nameof(SetupPageViewModel)}: Failed to submit form: {hashPasswordResult.Message}", saveUserDataResult.Exception);

        return true;
    }

    public async Task<bool> ImportBackupDataAsync()
    {
        Result<BackupData?> loadBackupResult = await backupService.LoadBackupFromFilePickerAsync();
        if (!loadBackupResult.IsSuccessful)
            throw new Exception($"{nameof(SetupPageViewModel)}: Failed to load backup data: {loadBackupResult.Message}", loadBackupResult.Exception);

        return true;
    }
}
