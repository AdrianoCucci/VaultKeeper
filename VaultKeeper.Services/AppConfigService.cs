using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services;

public class AppConfigService(
    ILogger<AppConfigService> logger,
    ICache<AppConfigData> configCache,
    IEncryptionService encryptionService,
    IRepository<VaultItem> vaultItemRepository,
    IAppDataService appDataService) : IAppConfigService
{
    public AppConfigData GetConfigDataOrDefault()
    {
        logger.LogInformation(nameof(GetConfigDataOrDefault));
        return configCache.Get() ?? AppConfigData.Default;
    }

    public async Task<Result<AppConfigData>> LoadSavedAppConfigAsync()
    {
        logger.LogInformation(nameof(LoadSavedAppConfigAsync));

        try
        {
            Result<SavedData<AppConfigData>?> loadAppConfigResult = await appDataService.LoadAppConfigDataAsync();
            if (!loadAppConfigResult.IsSuccessful)
                return loadAppConfigResult.WithValue<AppConfigData>().Logged(logger);

            AppConfigData appConfig = loadAppConfigResult.Value?.Data ?? AppConfigData.Default;
            string? encryptionKeyPath = appConfig.EncryptionKeyPath;

            if (!string.IsNullOrWhiteSpace(encryptionKeyPath))
            {
                Result useKeyFileResult = encryptionService.UseEncryptionKeyFile(encryptionKeyPath);
                if (!useKeyFileResult.IsSuccessful)
                    return useKeyFileResult.WithValue<AppConfigData>().Logged(logger);
            }

            configCache.Set(appConfig);

            return appConfig.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<AppConfigData>().Logged(logger);
        }
    }

    public async Task<Result<AppConfigData>> SetEncryptionKeyFilePathAsync(string? filePath)
    {
        logger.LogInformation(nameof(SetEncryptionKeyFilePathAsync));

        if (string.IsNullOrWhiteSpace(filePath))
        {
            if (!encryptionService.IsUsingDefaultEncryptionKey())
            {
                Result reEncryptEntitiesResult = await ReEncryptEntitiesAsync(filePath);
                if (!reEncryptEntitiesResult.IsSuccessful)
                    return reEncryptEntitiesResult.WithValue<AppConfigData>();
            }
        }
        else
        {
            Result validateKeyFileResult = encryptionService.VerifyValidEncryptionKeyFile(filePath);
            if (!validateKeyFileResult.IsSuccessful)
                return validateKeyFileResult.WithValue<AppConfigData>().Logged(logger);

            Result reEncryptEntitiesResult = await ReEncryptEntitiesAsync(filePath);
            if (!reEncryptEntitiesResult.IsSuccessful)
                return reEncryptEntitiesResult.WithValue<AppConfigData>();
        }

        AppConfigData config = GetConfigDataOrDefault() with { EncryptionKeyPath = filePath };
        configCache.Set(config);

        Result<SavedData<AppConfigData>?> saveConfigResult = await appDataService.SaveAppConfigDataAsync(config);
        if (!saveConfigResult.IsSuccessful)
            return saveConfigResult.WithValue<AppConfigData>().Logged(logger);

        return config.ToOkResult().Logged(logger);
    }

    private async Task<Result> ReEncryptEntitiesAsync(string? encryptionKeyFilePath)
    {
        try
        {
            IEnumerable<VaultItem> vaultItems = await vaultItemRepository.GetManyAsync();
            Result decryptResult = encryptionService.UsingEncryptionScope(scope =>
            {
                foreach (var item in vaultItems)
                {
                    item.Value = scope.Decrypt(item.Value);
                }
            });

            if (!decryptResult.IsSuccessful)
                return decryptResult;

            if (string.IsNullOrWhiteSpace(encryptionKeyFilePath))
                encryptionService.UseSystemEncryptionKey();
            else
                encryptionService.UseEncryptionKeyFile(encryptionKeyFilePath);

            Result encryptResult = encryptionService.UsingEncryptionScope(scope =>
            {
                foreach (var item in vaultItems)
                {
                    item.Value = scope.Encrypt(item.Value);
                }
            });

            if (!encryptResult.IsSuccessful)
                return encryptResult;

            await vaultItemRepository.SetAllAsync(vaultItems);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult();
        }
    }
}
