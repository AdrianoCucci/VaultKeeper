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

public class SetupViewModel(IAppDataService appDataService, ISecurityService securityService, IPlatformService platformService) : ViewModelBase
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

        Result<SavedData<UserData>?> saveUserDataResult = await appDataService.SaveUserDataAsync(userData, updateUserCache: true);
        if (!saveUserDataResult.IsSuccessful)
            throw new Exception($"{nameof(SetupViewModel)}: Failed to submit form: {hashPasswordResult.Message}", saveUserDataResult.Exception);

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

        string filePath = files[0].Path.AbsolutePath;
        Result<SavedData<BackupData>?> loadBackupResult = await appDataService.LoadBackupAsync(filePath);
        if (!loadBackupResult.IsSuccessful)
            throw new Exception($"{nameof(SetupViewModel)}: Failed to load backup data: {loadBackupResult.Message}", loadBackupResult.Exception);

        return true;
    }
}
