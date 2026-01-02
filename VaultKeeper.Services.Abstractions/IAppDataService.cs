using System;
using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.ApplicationData.Files;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.Services.Abstractions;

public interface IAppDataService
{
    AppFileDefinition GetFileDefinition(AppFileType fileType);
    Task<Result> SaveAllDataAsync();
    Task<Result<SavedData<UserData>?>> SaveUserDataAsync(UserData? userData = null, bool updateUserCache = false);
    Task<Result<SavedData<EntityData>>> SaveEntityDataAsync(EntityData? entityData = null, UserData? relatedUserData = null);
    Task<Result<SavedData<UserData>?>> LoadUserDataAsync(bool updateUserCache = false);
    Task<Result<SavedData<EntityData>?>> LoadEntityDataAsync(string? filePath = null, Guid? forUserId = null, bool updateRepositories = false);
    Task<Result> ClearEntityDataAsync();
    Task<Result<SavedData<BackupData>?>> SaveBackupAsync(BackupSettings backupSettings);
    Task<Result<SavedData<BackupData>?>> LoadBackupAsync(string filePath);
}