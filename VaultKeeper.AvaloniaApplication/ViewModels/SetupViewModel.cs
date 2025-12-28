using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.ApplicationData.Files;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public class SetupViewModel(
    IAppDataService appDataService,
    ICache<UserData> userDataCache,
    ISecurityService securityService,
    IPlatformService platformService) : ViewModelBase
{
    public SetupForm Form { get; } = new();

    public async Task<bool> SubmitFormAsync()
    {
        if (!Form.Validate()) return false;

        Result<string> hashPasswordResult = securityService.CreateHash(Form.PasswordInput!);
        if (!hashPasswordResult.IsSuccessful)
            throw new Exception($"{nameof(SetupViewModel)}: Failed to submit form: {hashPasswordResult.Message}", hashPasswordResult.Exception);

        UserData userData = new()
        {
            UserId = Guid.NewGuid(),
            MainPasswordHash = hashPasswordResult.Value!
        };

        Result<SavedData<UserData>?> saveUserDataResult = await appDataService.SaveUserDataAsync(userData);
        if (!saveUserDataResult.IsSuccessful)
            throw new Exception($"{nameof(SetupViewModel)}: Failed to submit form: {hashPasswordResult.Message}", saveUserDataResult.Exception);

        userDataCache.Set(userData);
        return true;
    }

    public async Task<bool> ImportBackupDataAsync()
    {
        AppFileDefinition backupFileDefinition = appDataService.GetFileDefinition(AppFileType.Backup);

        IReadOnlyList<IStorageFile> files = await platformService.OpenFilePickerAsync(new()
        {
            Title = "Import Backup",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new(backupFileDefinition.Name)
                {
                    Patterns = [$"*{backupFileDefinition.Extension}"]
                }
            ]
        });

        if (files.Count < 1)
            return false;

        // TODO: Process file import.
        return false;
    }
}
