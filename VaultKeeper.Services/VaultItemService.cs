using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Models.VaultItems.Extensions;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class VaultItemService(IRepository<VaultItem> repository, ISecurityService securityService, ILogger<VaultItemService> logger) : IVaultItemService
{
    public async Task<Result<IEnumerable<VaultItem>>> LoadAllAsync()
    {
        logger.LogInformation(nameof(LoadAllAsync));

        try
        {
            IEnumerable<VaultItem> items = await repository.GetManyAsync();
            return items.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<VaultItem>>().Logged(logger);
        }
    }

    public async Task<Result<IEnumerable<VaultItem>>> GetManyAsync(ReadQuery<VaultItem>? query = null)
    {
        logger.LogInformation(nameof(GetManyAsync));

        try
        {
            var items = await repository.GetManyAsync(query);
            return items.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<VaultItem>>().Logged(logger);
        }
    }

    public async Task<Result<VaultItem>> AddAsync(NewVaultItem vaultItem, bool encrypt = false)
    {
        logger.LogInformation(nameof(AddAsync));

        try
        {
            VaultItem model = vaultItem.ToVaultItem() with { Id = Guid.NewGuid() };

            Result validateResult = await ValidateUpsertAsync(model);
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

            Result validateResult = await ValidateUpsertAsync(vaultItem);
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

    private async Task<Result> ValidateUpsertAsync(VaultItem vaultItem)
    {
        if (string.IsNullOrWhiteSpace(vaultItem.Name))
            return Result.Failed(ResultFailureType.BadRequest, "Name is required.");

        if (string.IsNullOrWhiteSpace(vaultItem.Value))
            return Result.Failed(ResultFailureType.BadRequest, "Value is required.");

        bool isDuplicate = await repository.HasAnyAsync(new()
        {
            Where = x =>
                vaultItem.Name == x.Name &&
                vaultItem.Id != x.Id &&
                vaultItem.GroupId == x.GroupId
        });

        if (isDuplicate)
            return Result.Failed<VaultItem>(ResultFailureType.Conflict, $"Another Vault Item named \"{vaultItem.Name}\" already exists.");

        return Result.Ok();
    }
}
