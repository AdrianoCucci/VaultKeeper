using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Models.VaultItems.Extensions;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.VaultItems;

namespace VaultKeeper.Services.VaultItems;

public class VaultItemService(
    IRepository<VaultItem> repository,
    IVaultItemValidatorService validatorService,
    ISecurityService securityService,
    ILogger<VaultItemService> logger) : IVaultItemService
{
    public async Task<Result<CountedData<VaultItem>>> GetManyCountedAsync(ReadQuery<VaultItem>? query = null)
    {
        logger.LogInformation(nameof(GetManyCountedAsync));

        try
        {
            CountedData<VaultItem> items = await repository.GetManyCountedAsync(query);
            return items.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<CountedData<VaultItem>>().Logged(logger);
        }
    }

    public async Task<Result<VaultItem>> AddAsync(NewVaultItem vaultItem, bool encrypt = false)
    {
        logger.LogInformation(nameof(AddAsync));

        try
        {
            VaultItem model = vaultItem.ToVaultItem() with { Id = Guid.NewGuid() };

            Result validateResult = await validatorService.ValidateUpsertAsync(model);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue(model).Logged(logger);

            if (encrypt)
            {
                Result<string> encryptValueResult = securityService.Encrypt(model.Value);
                if (!encryptValueResult.IsSuccessful)
                    return encryptValueResult.WithValue(model).Logged(logger);

                model.Value = encryptValueResult.Value!;
            }

            model = await repository.AddAsync(model);

            return model.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<VaultItem>().Logged(logger);
        }
    }

    public async Task<Result<VaultItem>> UpdateAsync(VaultItem vaultItem, bool encrypt = false)
    {
        logger.LogInformation(nameof(UpdateAsync));

        try
        {
            VaultItem? existingModel = await repository.GetFirstOrDefaultAsync(new()
            {
                Where = x => x.Id == vaultItem.Id
            });

            if (existingModel == null)
                return Result.Failed<VaultItem>(ResultFailureType.NotFound, $"Vault Item ID does not exist: ({vaultItem.Id})").Logged(logger);

            Result validateResult = await validatorService.ValidateUpsertAsync(vaultItem);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue(vaultItem).Logged(logger);

            VaultItem updateModel = vaultItem with { };

            if (encrypt)
            {
                Result<string> encryptValueResult = securityService.Encrypt(updateModel.Value);
                if (!encryptValueResult.IsSuccessful)
                    return encryptValueResult.WithValue(updateModel).Logged(logger);

                updateModel.Value = encryptValueResult.Value!;
            }

            updateModel = (await repository.UpdateAsync(existingModel, updateModel))!;

            return updateModel.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<VaultItem>().Logged(logger);
        }
    }

    public async Task<Result> DeleteAsync(VaultItem vaultItem)
    {
        logger.LogInformation(nameof(DeleteAsync));

        try
        {
            bool didRemove = await repository.RemoveAsync(vaultItem);
            if (!didRemove)
                return Result.Failed<VaultItem>(ResultFailureType.NotFound, $"Vault Item ID does not exist: ({vaultItem.Id})").Logged(logger);

            return Result.Ok($"Vault Item deleted successfuly (ID: {vaultItem.Id}).").Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }
}
