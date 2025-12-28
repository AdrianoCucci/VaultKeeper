using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.ApplicationData.Files;
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

    public async Task<Result<SavedData<UserData>?>> SaveUserDataAsync(UserData? userData = null)
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
            string entityDataPath = string.IsNullOrWhiteSpace(filePath) ? CreateSaveDataPath(AppFileType.Entities) : filePath;

            Result<SavedData<EntityData>?> loadDataResult = LoadDataInternal<EntityData>(entityDataPath);
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
            {
                if (!loadedData.VaultItems.IsNullOrEmpty())
                    await vaultItemRepository.SetAllAsync(loadedData.VaultItems!);

                if (!loadedData.Groups.IsNullOrEmpty())
                    await groupRepository.SetAllAsync(loadedData.Groups!);
            }

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
            if (entityData == null)
            {
                IEnumerable<VaultItem> vaultItems = await vaultItemRepository.GetManyAsync();
                IEnumerable<Group> groups = await groupRepository.GetManyAsync();

                entityData = new()
                {
                    VaultItems = [.. vaultItems],
                    Groups = [.. groups]
                };
            }

            if (entityData.VaultItems.IsNullOrEmpty() && entityData.Groups.IsNullOrEmpty())
                entityData = new();

            SavedData<EntityData> savedData = new()
            {
                Data = entityData with { UserId = relatedUserData?.UserId ?? entityData.UserId },
                Metadata = new() { Version = _saveDataVersion }
            };

            string filePath = string.IsNullOrWhiteSpace(relatedUserData?.CustomEntitiesDataPath)
                ? CreateSaveDataPath(AppFileType.Entities)
                : relatedUserData.CustomEntitiesDataPath;

            var saveResult = SaveDataInternal(filePath, savedData);

            return saveResult.WithValue(savedData).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<EntityData>>().Logged(logger);
        }
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

        return dataDeserializeResult;
    }

    private string CreateSaveDataPath(AppFileType fileType)
    {
        AppFileDefinition definition = _fileDefinitionsDict[fileType];
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataDirectory, definition.FullName);
    }
}
