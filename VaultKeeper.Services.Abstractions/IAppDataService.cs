using System;
using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.Services.Abstractions;

public interface IAppDataService
{
    Task<Result> SaveAllDataAsync(BackupData? backupData = null);
    Task<Result<SavedData<AppConfigData>?>> SaveAppConfigDataAsync(AppConfigData? appConfigData = null, bool updateCache = false);
    Task<Result<SavedData<AppConfigData>?>> LoadAppConfigDataAsync(bool updateCache = false);
    Task<Result<SavedData<UserData>?>> SaveUserDataAsync(UserData? userData = null, UserSettings? userSettings = null, bool updateCaches = false);
    Task<Result<SavedData<UserData>?>> LoadUserDataAsync(bool updateCaches = false);
    Task<Result<SavedData<EntityData>?>> SaveEntityDataAsync(EntityData? entityData = null, UserData? relatedUserData = null);
    Task<Result<SavedData<EntityData>?>> LoadEntityDataAsync(string? filePath = null, Guid? forUserId = null, bool updateRepositories = false);
    Task<Result> DeleteEntityDataAsync();
    Task<Result> ClearCachedEntityDataAsync();
    Task<Result<SavedData<BackupData>?>> SaveBackupAsync(BackupSettings backupSettings);
    Task<Result<SavedData<BackupData>?>> LoadBackupAsync(string filePath);
}