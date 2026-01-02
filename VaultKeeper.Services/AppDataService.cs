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

namespace VaultKeeper.Services;

public class AppDataService(
    ILogger<AppDataService> logger,
    IFileService fileService,
    IJsonService jsonService,
    ISecurityService securityService,
    IRepository<VaultItem> vaultItemRepository,
    IRepository<Group> groupRepository,
    ICache<UserData> userDataCache) : IAppDataService
{
    private const string _dataExtension = ".dat";
    private const string _backupExtension = ".bak";
    private const string _commonExtension = ".vk";

    private static readonly Dictionary<AppFileType, AppFileDefinition> _fileDefinitionsDict = new()
    {
        {
            AppFileType.User,
            new()
            {
                FileType = AppFileType.User,
                Name = "User",
                Extension = $"{_dataExtension}{_commonExtension}"
            }
        },
        {
            AppFileType.Entities,
            new()
            {
                FileType = AppFileType.Entities,
                Name = "Entities",
                Extension = $"{_dataExtension}{_commonExtension}"
            }
        },
        {
            AppFileType.Backup,
            new()
            {
                FileType = AppFileType.Backup,
                Name = "VaultKeeper",
                Extension = $"{_backupExtension}{_commonExtension}"
            }
        }
    };

    public string DataDirectory { get; } = "VaultKeeper";

    private const int _saveDataVersion = 1;

    public AppFileDefinition GetFileDefinition(AppFileType fileType)
    {
        logger.LogInformation($"{nameof(GetFileDefinition)} | {nameof(fileType)}: {{fileType}}", fileType);
        return _fileDefinitionsDict[fileType];
    }

    public async Task<Result> SaveAllDataAsync()
    {
        logger.LogInformation(nameof(SaveAllDataAsync));

        try
        {
            Result<SavedData<UserData>?> saveUserDataResult = await SaveUserDataAsync();
            if (!saveUserDataResult.IsSuccessful)
                return saveUserDataResult.Logged(logger);

            UserData? userData = saveUserDataResult.Value?.Data;

            Result<SavedData<EntityData>> saveEntitiesResult = await SaveEntityDataAsync(relatedUserData: userData);
            if (!saveEntitiesResult.IsSuccessful)
                return saveEntitiesResult.Logged(logger);

            return Result.Ok().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public async Task<Result<SavedData<UserData>?>> SaveUserDataAsync(UserData? userData = null, bool updateUserCache = false)
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

            SavedData<UserData> savedData = new()
            {
                Data = userData,
                Metadata = new() { Version = _saveDataVersion }
            };

            string userDataPath = CreateSaveDataPath(AppFileType.User);
            Result saveResult = SaveDataInternal(userDataPath, savedData);

            if (saveResult.IsSuccessful && updateUserCache)
                userDataCache.Set(userData);

            return saveResult.WithValue(savedData).Logged(logger)!;
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<UserData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<UserData>?>> LoadUserDataAsync(bool updateUserCache = false)
    {
        logger.LogInformation(nameof(LoadUserDataAsync));

        try
        {
            string userDataPath = CreateSaveDataPath(AppFileType.User);
            Result<SavedData<UserData>?> loadDataResult = LoadDataInternal<UserData>(userDataPath);

            if (updateUserCache && loadDataResult.IsSuccessful && loadDataResult.Value != null)
                userDataCache.Set(loadDataResult.Value.Data);

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

    public async Task<Result<SavedData<EntityData>>> SaveEntityDataAsync(EntityData? entityData = null, UserData? relatedUserData = null)
    {
        logger.LogInformation(nameof(SaveEntityDataAsync));

        try
        {
            entityData ??= await CreateEntityDataAsync();

            if (entityData.VaultItems.IsNullOrEmpty() && entityData.Groups.IsNullOrEmpty())
                entityData = new();

            SavedData<EntityData> savedData = new()
            {
                Data = entityData with { UserId = relatedUserData?.UserId ?? entityData.UserId },
                Metadata = new() { Version = _saveDataVersion }
            };

            string filePath = CreateSaveDataPath(AppFileType.Entities);
            Result saveResult = SaveDataInternal(filePath, savedData);

            return saveResult.WithValue(savedData).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<EntityData>>().Logged(logger);
        }
    }

    public async Task<Result> ClearEntityDataAsync()
    {
        logger.LogInformation(nameof(ClearEntityDataAsync));

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
            string filePath = CreateSaveDataPath(AppFileType.Backup, directory, DateTime.UtcNow.ToString("_yyyMMdd_HHmmss"));

            UserData? userData = userDataCache.Get();
            if (userData == null)
            {
                logger.LogWarning($"{nameof(UserData)} not set in cache - cancelling backup process.");
                return Result.Ok<SavedData<BackupData>?>(null).Logged(logger);
            }

            EntityData entityData = await CreateEntityDataAsync();

            SavedData<BackupData> savedData = new()
            {
                Data = new()
                {
                    UserData = userData,
                    EntityData = entityData with { UserId = userData.UserId }
                },
                Metadata = new() { Version = _saveDataVersion }
            };

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
            var backupDefinition = _fileDefinitionsDict[AppFileType.Backup];
            if (filePath?.EndsWith(backupDefinition.Extension) != true)
                return Result.Failed<SavedData<BackupData>?>(ResultFailureType.BadRequest, $"File path does not point to a valid backup file: \"{filePath}\"").Logged(logger);

            Result<SavedData<BackupData>?> loadResult = LoadDataInternal<BackupData>(filePath);
            if (!loadResult.IsSuccessful)
                return loadResult.Logged(logger);

            SavedData<BackupData> loadedData = loadResult.Value!;
            UserData userData = loadedData.Data.UserData;

            Result<SavedData<UserData>?> saveUserDataResult = await SaveUserDataAsync(userData, updateUserCache: true);
            if (!saveUserDataResult.IsSuccessful)
                return saveUserDataResult.WithValue<SavedData<BackupData>?>().Logged(logger);

            Result<SavedData<EntityData>> saveEntityDataResult = await SaveEntityDataAsync(loadedData.Data.EntityData, userData);
            if (!saveEntityDataResult.IsSuccessful)
                return saveEntityDataResult.WithValue<SavedData<BackupData>?>().Logged(logger);

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

        AppFileDefinition backupDefinition = _fileDefinitionsDict[AppFileType.Backup];
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

    private async Task UpdateEntityRepositoriesAsync(EntityData entityData)
    {
        if (!entityData.VaultItems.IsNullOrEmpty())
            await vaultItemRepository.SetAllAsync(entityData.VaultItems!);

        if (!entityData.Groups.IsNullOrEmpty())
            await groupRepository.SetAllAsync(entityData.Groups!);
    }

    private Result SaveDataInternal<T>(string filePath, SavedData<T> savedData)
    {
        Result<string> dataSerializeResult = jsonService.Serialize(savedData);
        if (!dataSerializeResult.IsSuccessful)
            return dataSerializeResult;

        Result<string> dataEncryptResult = securityService.Encrypt(dataSerializeResult.Value!);
        if (!dataEncryptResult.IsSuccessful)
            return dataEncryptResult;

        Result saveResult = fileService.WriteFileText(filePath, dataEncryptResult.Value!);

        return saveResult;
    }

    private Result<SavedData<T>?> LoadDataInternal<T>(string filePath)
    {
        Result<string> readFileResult = fileService.ReadFileText(filePath);
        if (!readFileResult.IsSuccessful)
        {
            if (readFileResult.FailureType == ResultFailureType.NotFound)
                return Result.Ok<SavedData<T>?>(null);

            return readFileResult.WithValue<SavedData<T>?>();
        }

        Result<string> dataDecryptResult = securityService.Decrypt(readFileResult.Value!);
        if (!dataDecryptResult.IsSuccessful)
            return dataDecryptResult.WithValue<SavedData<T>?>();

        Result<SavedData<T>?> dataDeserializeResult = jsonService.Deserialize<SavedData<T>?>(dataDecryptResult.Value!);
        if (!dataDeserializeResult.IsSuccessful)
            dataDeserializeResult = dataDeserializeResult with
            {
                Message = "Saved file data could not be deserialized - data may be corrupted."
            };

        return dataDeserializeResult;
    }

    private string CreateSaveDataPath(AppFileType fileType, string? directory = null, string? fileNameSuffix = null)
    {
        AppFileDefinition definition = _fileDefinitionsDict[fileType];
        directory ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataDirectory);
        string fileName = definition.Name;

        if (!string.IsNullOrWhiteSpace(fileNameSuffix))
            fileName = $"{fileName}{fileNameSuffix}";

        fileName += definition.Extension;

        return Path.Combine(directory, fileName).Replace('\\', '/');
    }
}
