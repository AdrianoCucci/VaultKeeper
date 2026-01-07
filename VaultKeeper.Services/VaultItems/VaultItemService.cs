using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<Result<long>> DeleteManyAsync(IEnumerable<VaultItem> vaultItems)
    {
        logger.LogInformation(nameof(DeleteManyAsync));

        try
        {
            IEnumerable<Guid> ids = vaultItems.Select(x => x.Id).Distinct();
            IEnumerable<VaultItem> existingItems = await repository.GetManyAsync(new() { Where = x => ids.Contains(x.Id) });
            VaultItem[] notFoundItems = [.. vaultItems.Where(x => !existingItems.Any(y => x.Id == y.Id))];

            if (notFoundItems.Length > 0)
                return Result.Failed<long>(ResultFailureType.NotFound, $"{notFoundItems.Length} Vault Items do not exist: [{string.Join(", ", notFoundItems.Select(x => x.Id))}]").Logged(logger);

            long deleteCount = await repository.RemoveManyAsync(existingItems);

            return deleteCount.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<long>().Logged(logger);
        }
    }

}
