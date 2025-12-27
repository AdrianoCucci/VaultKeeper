using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models;
using VaultKeeper.Models.ApplicationData;
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
    private const int _saveDataVersion = 1;
    private const string _appDataDirectory = "VaultKeeper";
    private const string _userDataFile = "VaultKeeper.user.dat";
    private const string _entitiesDataFile = "VaultKeeper.entities.dat";

    public async Task<Result> SaveAllDataAsync()
    {
        logger.LogInformation(nameof(SaveAllDataAsync));

        try
        {
            Result<SavedData<UserData>?> saveUserDataResult = await SaveUserDataAsync();
            if (!saveUserDataResult.IsSuccessful)
                return saveUserDataResult.Logged(logger);

            UserData? userData = saveUserDataResult.Value?.Data;

            Result<SavedData<EntityData>> saveEntitiesResult = await SaveEntityDataAsync(filePath: userData?.CustomEntitiesDataPath);
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
            if (userData == null)
            {
                Result<UserData?> getUserDataResult = userDataCache.Get();
                if (!getUserDataResult.IsSuccessful)
                    return getUserDataResult.WithValue<SavedData<UserData>?>().Logged(logger);

                userData = getUserDataResult.Value;
            }

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

            string userDataPath = CreateSaveDataPath(_userDataFile);
            Result saveResult = SaveDataInternal(userDataPath, savedData);

            return saveResult.WithValue(savedData).Logged(logger)!;
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<UserData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<UserData>?>> LoadUserDataAsync()
    {
        logger.LogInformation(nameof(LoadUserDataAsync));

        try
        {
            string userDataPath = CreateSaveDataPath(_userDataFile);
            Result<SavedData<UserData>?> loadDataResult = LoadDataInternal<UserData>(userDataPath);

            return loadDataResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<UserData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<EntityData>?>> LoadEntityDataAsync(string? filePath = null)
    {
        logger.LogInformation(nameof(LoadEntityDataAsync));

        try
        {
            string entityDataPath = string.IsNullOrWhiteSpace(filePath) ? CreateSaveDataPath(_userDataFile) : filePath;
            Result<SavedData<EntityData>?> loadDataResult = LoadDataInternal<EntityData>(entityDataPath);

            return loadDataResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<SavedData<EntityData>?>().Logged(logger);
        }
    }

    public async Task<Result<SavedData<EntityData>>> SaveEntityDataAsync(EntityData? entityData = null, string? filePath = null)
    {
        logger.LogInformation(nameof(SaveEntityDataAsync));

        try
        {
            if (entityData == null)
            {
                // Entities
                IEnumerable<VaultItem> vaultItems = await vaultItemRepository.GetManyAsync();
                IEnumerable<Group> groups = await groupRepository.GetManyAsync();

                entityData = new()
                {
                    VaultItems = [.. vaultItems],
                    Groups = [.. groups]
                };
            }

            SavedData<EntityData> savedData = new()
            {
                Data = entityData,
                Metadata = new() { Version = _saveDataVersion }
            };

            filePath ??= CreateSaveDataPath(_entitiesDataFile);
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

        Result saveResult = fileService.WriteFileText(filePath, dataEncryptResult.Value!, FileAttributes.ReadOnly);

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

    private static string CreateSaveDataPath(string file) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _appDataDirectory, file);
}
