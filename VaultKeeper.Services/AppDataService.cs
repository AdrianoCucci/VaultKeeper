using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.ApplicationData.Files;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.Settings;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.DataFormatting;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services;

public class AppDataService(
    ILogger<AppDataService> logger,
    IFileService fileService,
    IJsonService jsonService,
    IEncryptionService encryptionService,
    IAppFileDefinitionService fileDefinitionService,
    IRepository<VaultItem> vaultItemRepository,
    IRepository<Group> groupRepository,
    ICache<AppConfigData> appConfigCache,
    ICache<UserData> userDataCache,
    ICache<UserSettings> userSettingsCache) : IAppDataService
{
    public string DataDirectory { get; } = "VaultKeeper";

    private const int _saveDataVersion = 1;

    public async Task<Result> SaveAllDataAsync(BackupData? backupData = null)
    {
        logger.LogInformation(nameof(SaveAllDataAsync));

        try
        {
            Result<SavedData<AppConfigData>?> saveAppConfigResult = await SaveAppConfigDataAsync(backupData?.AppConfigData);
            if (!saveAppConfigResult.IsSuccessful)
                return saveAppConfigResult.Logged(logger);

            Result<SavedData<UserData>?> saveUserDataResult = await SaveUserDataAsync(backupData?.UserData, backupData?.UserData?.Settings);
            if (!saveUserDataResult.IsSuccessful)
                return saveUserDataResult.Logged(logger);

            UserData? userData = saveUserDataResult.Value?.Data;
            if (userData != null)
            {
                Result<SavedData<EntityData>?> saveEntitiesResult = await SaveEntityDataAsync(backupData?.EntityData, relatedUserData: userData);
                if (!saveEntitiesResult.IsSuccessful)
                    return saveEntitiesResult.Logged(logger);
            }

            return Result.Ok().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public async Task<Result<SavedData<AppConfigData>?>> SaveAppConfigDataAsync(AppConfigData? appConfigData = null, bool updateCache = false)
    {
        try
        {
            appConfigData ??= appConfigCache.Get() ?? AppConfigData.Default;

            SavedData<AppConfigData> savedData = CreateSavedData(appConfigData);
            string userDataPath = CreateSaveDataPath(AppFileType.AppConfig);
            Result saveResult = SaveDataInternal(userDataPath, savedData, encrypt: false);

            if (saveResult.IsSuccessful && updateCache)
                appConfigCache.Set(savedData.Data);

            return saveResult.WithValue(savedData).Logged(logger)!;
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<AppConfigData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<AppConfigData>?>> LoadAppConfigDataAsync(bool updateCache = false)
    {
        logger.LogInformation(nameof(LoadAppConfigDataAsync));

        try
        {
            string appConfigPath = CreateSaveDataPath(AppFileType.AppConfig);
            Result<SavedData<AppConfigData>?> loadDataResult = LoadDataInternal<AppConfigData>(appConfigPath, decrypt: false);

            if (updateCache && loadDataResult.IsSuccessful && loadDataResult.Value != null)
                appConfigCache.Set(loadDataResult.Value.Data);

            return loadDataResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<AppConfigData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<UserData>?>> SaveUserDataAsync(UserData? userData = null, UserSettings? userSettings = null, bool updateCaches = false)
    {
        logger.LogInformation(nameof(SaveUserDataAsync));

        try
        {
            userData ??= userDataCache.Get();

            if (userData == null)
            {
                logger.LogInformation($"No user data provided, and none found in cache - nothing to save.");
                return userData.ToOkResult().WithValue<SavedData<UserData>?>().Logged(logger);
            }

            userSettings ??= userSettingsCache.Get();

            SavedData<UserData> savedData = CreateSavedData(userData with { Settings = userSettings });
            string userDataPath = CreateSaveDataPath(AppFileType.User);
            Result saveResult = SaveDataInternal(userDataPath, savedData);

            if (saveResult.IsSuccessful && updateCaches)
                UpdateUserCaches(userData, userSettings);

            return saveResult.WithValue(savedData).Logged(logger)!;
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<UserData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<UserData>?>> LoadUserDataAsync(bool updateCaches = false)
    {
        logger.LogInformation(nameof(LoadUserDataAsync));

        try
        {
            string userDataPath = CreateSaveDataPath(AppFileType.User);
            Result<SavedData<UserData>?> loadDataResult = LoadDataInternal<UserData>(userDataPath);

            if (updateCaches && loadDataResult.IsSuccessful && loadDataResult.Value != null)
            {
                UserData userData = loadDataResult.Value.Data;
                UserSettings? userSettings = userData.Settings;

                UpdateUserCaches(userData, userSettings);
            }

            return loadDataResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<UserData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<EntityData>?>> LoadEntityDataAsync(string? filePath = null, Guid? forUserId = null, bool updateRepositories = false)
    {
        logger.LogInformation(nameof(LoadEntityDataAsync));

        try
        {
            filePath ??= CreateSaveDataPath(AppFileType.Entities);

            Result<SavedData<EntityData>?> loadDataResult = LoadDataInternal<EntityData>(filePath);
            if (!loadDataResult.IsSuccessful)
                return loadDataResult.Logged(logger);

            EntityData? loadedData = loadDataResult.Value?.Data;
            if (loadedData == null)
                return loadDataResult.Logged(logger);

            if (forUserId.HasValue && loadedData.UserId != forUserId.Value)
            {
                logger.LogWarning("Entity data was loaded, but user ID does not match. Discarding data.");
                return loadDataResult.WithValue<SavedData<EntityData>?>(null).Logged(logger);
            }

            if (updateRepositories)
                await UpdateEntityRepositoriesAsync(loadedData);

            return loadDataResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<EntityData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<EntityData>?>> SaveEntityDataAsync(EntityData? entityData = null, UserData? relatedUserData = null)
    {
        logger.LogInformation(nameof(SaveEntityDataAsync));

        try
        {
            entityData ??= await CreateEntityDataAsync();
            relatedUserData ??= userDataCache.Get();

            if (relatedUserData == null)
                return Result.Failed<SavedData<EntityData>?>(ResultFailureType.BadRequest, $"{nameof(UserData)} not provided and fallback value not set in cache.");

            if (entityData.VaultItems.IsNullOrEmpty() && entityData.Groups.IsNullOrEmpty())
                entityData = new();

            SavedData<EntityData> savedData = CreateSavedData(entityData with { UserId = relatedUserData?.UserId ?? entityData.UserId });
            string filePath = CreateSaveDataPath(AppFileType.Entities);
            Result saveResult = SaveDataInternal(filePath, savedData);

            return saveResult.WithValue<SavedData<EntityData>?>(savedData).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<EntityData>?>().Logged(logger);
        }
    }

    public async Task<Result> DeleteEntityDataAsync()
    {
        logger.LogInformation(nameof(DeleteEntityDataAsync));

        try
        {
            string filePath = CreateSaveDataPath(AppFileType.Entities);
            Result deleteResult = fileService.DeleteFileAsync(filePath);

            return deleteResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public async Task<Result> ClearCachedEntityDataAsync()
    {
        logger.LogInformation(nameof(ClearCachedEntityDataAsync));

        try
        {
            await Task.WhenAll(vaultItemRepository.RemoveAllAsync(), groupRepository.RemoveAllAsync());
            return Result.Ok().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public async Task<Result<SavedData<BackupData>?>> SaveBackupAsync(BackupSettings backupSettings)
    {
        string directory = backupSettings.BackupDirectory;
        logger.LogInformation($"{nameof(SaveBackupAsync)} | {nameof(directory)}: \"{{directory}}\"", directory);

        try
        {
            AppConfigData? appConfig = appConfigCache.Get();
            if (appConfig == null)
            {
                logger.LogWarning($"{nameof(AppConfigData)} not set in cache - cancelling backup process.");
                return Result.Ok<SavedData<BackupData>?>(null).Logged(logger);
            }

            UserData? userData = userDataCache.Get();
            if (userData == null)
            {
                logger.LogWarning($"{nameof(UserData)} not set in cache - cancelling backup process.");
                return Result.Ok<SavedData<BackupData>?>(null).Logged(logger);
            }

            UserSettings? userSettings = userSettingsCache.Get();
            EntityData entityData = await CreateEntityDataAsync();

            SavedData<BackupData> savedData = new()
            {
                Data = new()
                {
                    AppConfigData = appConfig,
                    UserData = userData with { Settings = userSettings },
                    EntityData = entityData with { UserId = userData.UserId }
                },
                Metadata = new() { Version = _saveDataVersion }
            };

            string filePath = CreateSaveDataPath(AppFileType.Backup, directory, DateTime.UtcNow.ToString("_yyyMMdd_HHmmss"));

            Result saveResult = SaveDataInternal(filePath, savedData);

            if (saveResult.IsSuccessful)
            {
                FileInfo[] existingBackups = GetBackupFiles(directory);
                if (existingBackups.Length > backupSettings.MaxBackups)
                    DeleteOldestFilesUpToCount(existingBackups, backupSettings.MaxBackups);
            }

            return saveResult.WithValue<SavedData<BackupData>?>(savedData).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<BackupData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<BackupData>?>> LoadBackupAsync(string filePath)
    {
        logger.LogInformation($"{nameof(LoadBackupAsync)} | {nameof(filePath)}: \"{{filePath}}\"", filePath);

        try
        {
            var backupDefinition = fileDefinitionService.GetFileDefinitionByType(AppFileType.Backup);
            if (filePath?.EndsWith(backupDefinition.Extension) != true)
                return Result.Failed<SavedData<BackupData>?>(ResultFailureType.BadRequest, $"File path does not point to a valid backup file: \"{filePath}\"").Logged(logger);

            Result<SavedData<BackupData>?> loadResult = LoadDataInternal<BackupData>(filePath);
            if (!loadResult.IsSuccessful)
                return loadResult.Logged(logger);

            SavedData<BackupData> loadedData = loadResult.Value!;

            Result saveAllDataResult = await SaveAllDataAsync(loadedData.Data);
            if (!saveAllDataResult.IsSuccessful)
                return saveAllDataResult.WithValue<SavedData<BackupData>?>().Logged(logger);

            await UpdateEntityRepositoriesAsync(loadedData.Data.EntityData);

            return loadResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<BackupData>?>().Logged(logger);
        }
    }

    private FileInfo[] GetBackupFiles(string directory)
    {
        logger.LogInformation($"{nameof(GetBackupFiles)} | directory: {{directory}}", directory);

        AppFileDefinition backupDefinition = fileDefinitionService.GetFileDefinitionByType(AppFileType.Backup);
        FileInfo[] backups = [..Directory.EnumerateFiles(directory)
            .Where(x => x.EndsWith(backupDefinition.Extension))
            .Select(x => new FileInfo(x))];

        return backups;
    }

    private void DeleteOldestFilesUpToCount(FileInfo[] files, int count)
    {
        logger.LogInformation($"{nameof(DeleteOldestFilesUpToCount)} | count: {{count}}", count);

        if (files.Length < 1)
            return;

        IEnumerable<FileInfo> filesToDelete = files.OrderByDescending(x => x.CreationTimeUtc).Skip(count);

        foreach (FileInfo file in filesToDelete)
        {
            File.Delete(file.FullName);
        }
    }

    private async Task<EntityData> CreateEntityDataAsync()
    {
        IEnumerable<VaultItem> vaultItems = await vaultItemRepository.GetManyAsync();
        IEnumerable<Group> groups = await groupRepository.GetManyAsync();

        return new()
        {
            VaultItems = [.. vaultItems],
            Groups = [.. groups]
        };
    }

    private void UpdateUserCaches(UserData userData, UserSettings? userSettings)
    {
        userDataCache.Set(userData with { });

        if (userSettings == null)
            userSettingsCache.Clear();
        else
            userSettingsCache.Set(userSettings with { });
    }

    private async Task UpdateEntityRepositoriesAsync(EntityData entityData) => await Task.WhenAll(
        vaultItemRepository.SetAllAsync(entityData.VaultItems ?? []),
        groupRepository.SetAllAsync(entityData.Groups ?? []));

    private Result SaveDataInternal<T>(string filePath, SavedData<T> savedData, bool encrypt = true)
    {
        Result<string> dataSerializeResult = jsonService.Serialize(savedData);
        if (!dataSerializeResult.IsSuccessful)
            return dataSerializeResult;

        string dataToSave = dataSerializeResult.Value!;

        if (encrypt)
        {
            Result<string> dataEncryptResult = encryptionService.Encrypt(dataToSave);
            if (!dataEncryptResult.IsSuccessful)
                return dataEncryptResult;

            dataToSave = dataEncryptResult.Value!;
        }

        Result saveResult = fileService.WriteFileText(filePath, dataToSave);

        return saveResult;
    }

    private Result<SavedData<T>?> LoadDataInternal<T>(string filePath, bool decrypt = true)
    {
        Result<string> readFileResult = fileService.ReadFileText(filePath);
        if (!readFileResult.IsSuccessful)
        {
            if (readFileResult.FailureType == ResultFailureType.NotFound)
                return Result.Ok<SavedData<T>?>(null);

            return readFileResult.WithValue<SavedData<T>?>();
        }

        string loadedData = readFileResult.Value!;

        if (decrypt)
        {
            Result<string> dataDecryptResult = encryptionService.Decrypt(readFileResult.Value!);
            if (!dataDecryptResult.IsSuccessful)
                return dataDecryptResult.WithValue<SavedData<T>?>();

            loadedData = dataDecryptResult.Value!;
        }

        Result<SavedData<T>?> dataDeserializeResult = jsonService.Deserialize<SavedData<T>?>(loadedData);
        if (!dataDeserializeResult.IsSuccessful)
            dataDeserializeResult = dataDeserializeResult with
            {
                Message = "Saved file data could not be deserialized - data may be corrupted."
            };

        return dataDeserializeResult;
    }

    private static SavedData<T> CreateSavedData<T>(T data) => new()
    {
        Data = data,
        Metadata = new() { Version = _saveDataVersion }
    };

    private string CreateSaveDataPath(AppFileType fileType, string? directory = null, string? fileNameSuffix = null)
    {
        AppFileDefinition definition = fileDefinitionService.GetFileDefinitionByType(fileType);
        directory ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataDirectory);
        string fileName = definition.Name;

        if (!string.IsNullOrWhiteSpace(fileNameSuffix))
            fileName = $"{fileName}{fileNameSuffix}";

        fileName += definition.Extension;

        return Path.Combine(directory, fileName).Replace('\\', '/');
    }
}
