using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class AppDataService(
    ILogger<AppDataService> logger,
    IFileService fileService,
    IJsonService jsonService,
    ISecurityService securityService,
    IRepository<VaultItem> vaultItemRepository) : IAppDataService
{
    private const string _appDataDirectory = "VaultKeeper";
    private const string _userDataFile = "VaultKeeper.user.dat";
    private const string _entitiesDataFile = "VaultKeeper.entities.dat";

    public async Task<Result> SaveDataAsync()
    {
        logger.LogInformation(nameof(SaveDataAsync));

        try
        {
            // User data
            UserData userData = new()
            {
                Version = 1,
                MainPassword = "TODO"
            };

            var hashPasswordResult = securityService.CreateHash(userData.MainPassword);
            if (!hashPasswordResult.IsSuccessful)
                return hashPasswordResult.Logged(logger);

            userData = userData with { MainPassword = hashPasswordResult.Value };

            string userDataPath = CreateSaveDataPath(_userDataFile);
            Result saveUserDataResult = SaveDataInternal(userDataPath, userData);
            if (!saveUserDataResult.IsSuccessful)
                return saveUserDataResult.Logged(logger);

            // Entities
            IEnumerable<VaultItem> vaultItems = await vaultItemRepository.GetManyAsync();

            EntityData entityData = new()
            {
                Version = 1,
                VaultItems = [.. vaultItems]
            };

            string entitiesDataPath = string.IsNullOrWhiteSpace(userData.CustomEntitiesDataPath)
                ? CreateSaveDataPath(_entitiesDataFile)
                : userData.CustomEntitiesDataPath;

            var saveEntitiesResult = SaveDataInternal(entitiesDataPath, entityData);

            return saveEntitiesResult.Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public async Task<Result<UserData>> LoadUserDataAsync()
    {
        logger.LogInformation(nameof(LoadUserDataAsync));

        try
        {
            string userDataPath = CreateSaveDataPath(_userDataFile);
            Result<UserData> loadDataResult = LoadDataInternal<UserData>(userDataPath);
            if (!loadDataResult.IsSuccessful)
                return loadDataResult.Logged(logger, failedAsWarning: loadDataResult.FailureType == ResultFailureType.NotFound);

            UserData userData = loadDataResult.Value!;

            return userData.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<UserData>().Logged(logger);
        }
    }

    public async Task<Result<EntityData>> LoadEntityDataAsync(string? filePath = null)
    {
        logger.LogInformation(nameof(LoadEntityDataAsync));

        try
        {
            string entityDataPath = string.IsNullOrWhiteSpace(filePath) ? CreateSaveDataPath(_userDataFile) : filePath;
            Result<EntityData> loadDataResult = LoadDataInternal<EntityData>(entityDataPath);
            if (!loadDataResult.IsSuccessful)
                return loadDataResult.Logged(logger, failedAsWarning: loadDataResult.FailureType == ResultFailureType.NotFound);

            EntityData entityData = loadDataResult.Value!;

            return entityData.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<EntityData>().Logged(logger);
        }
    }

    private Result SaveDataInternal<T>(string filePath, T fileData)
    {
        Result<string> dataSerializeResult = jsonService.Serialize(fileData);
        if (!dataSerializeResult.IsSuccessful)
            return dataSerializeResult;

        Result<string> dataEncryptResult = securityService.Encrypt(dataSerializeResult.Value!);
        if (!dataEncryptResult.IsSuccessful)
            return dataEncryptResult;

        Result saveResult = fileService.WriteFileText(filePath, dataEncryptResult.Value!, FileAttributes.ReadOnly);

        return saveResult;
    }

    private Result<T> LoadDataInternal<T>(string filePath)
    {
        Result<string> readFileResult = fileService.ReadFileText(filePath);
        if (!readFileResult.IsSuccessful)
            return readFileResult.WithValue<T>();

        Result<string> dataDecryptResult = securityService.Decrypt(readFileResult.Value!);
        if (!dataDecryptResult.IsSuccessful)
            return dataDecryptResult.WithValue<T>();

        Result<T> dataDeserializeResult = jsonService.Deserialize<T>(dataDecryptResult.Value!);

        return dataDeserializeResult;
    }

    private static string CreateSaveDataPath(string file) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _appDataDirectory, file);
}
