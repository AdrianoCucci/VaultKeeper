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
using VaultKeeper.Services.Abstractions.Security;
using VaultKeeper.Services.Abstractions.VaultItems;

namespace VaultKeeper.Services.VaultItems;

public class VaultItemService(
    IRepository<VaultItem> repository,
    IVaultItemValidatorService validatorService,
    IEncryptionService encryptionService,
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
                return validateResult.WithValue<VaultItem>().Logged(logger);

            if (encrypt)
            {
                Result<string> encryptValueResult = encryptionService.Encrypt(model.Value);
                if (!encryptValueResult.IsSuccessful)
                    return encryptValueResult.WithValue<VaultItem>().Logged(logger);

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

    public async Task<Result<IEnumerable<VaultItem>>> AddManyAsync(IEnumerable<NewVaultItem> vaultItems, bool encrypt = false)
    {
        logger.LogInformation(nameof(AddManyAsync));

        try
        {
            VaultItem[] models = [.. vaultItems.Select(x => x.ToVaultItem() with { Id = Guid.NewGuid() })];

            Result validateResult = await validatorService.ValidateUpsertManyAsync(models);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue<IEnumerable<VaultItem>>().Logged(logger);

            if (encrypt)
            {
                Result encryptResult = encryptionService.UsingEncryptionScope(scope =>
                {
                    foreach (var model in models)
                    {
                        model.Value = scope.Encrypt(model.Value);
                    }
                });

                if (!encryptResult.IsSuccessful)
                    return encryptResult.WithValue<IEnumerable<VaultItem>>().Logged(logger);
            }

            models = [.. await repository.AddManyAsync(models)];

            return Result.Ok<IEnumerable<VaultItem>>(models).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<VaultItem>>().Logged(logger);
        }
    }

    public async Task<Result<VaultItem>> UpdateAsync(VaultItem vaultItem, bool encrypt = false)
    {
        logger.LogInformation(nameof(UpdateAsync));

        try
        {
            VaultItem? existingItem = await repository.GetFirstOrDefaultAsync(new()
            {
                Where = x => x.Id == vaultItem.Id
            });

            if (existingItem == null)
                return Result.Failed<VaultItem>(ResultFailureType.NotFound, $"Vault Item ID does not exist: ({vaultItem.Id})").Logged(logger);

            Result validateResult = await validatorService.ValidateUpsertAsync(vaultItem);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue<VaultItem>().Logged(logger);

            VaultItem updateModel = vaultItem with { };

            if (encrypt)
            {
                Result<string> encryptValueResult = encryptionService.Encrypt(updateModel.Value);
                if (!encryptValueResult.IsSuccessful)
                    return encryptValueResult.WithValue<VaultItem>().Logged(logger);

                updateModel.Value = encryptValueResult.Value!;
            }

            updateModel = (await repository.UpdateAsync(existingItem, updateModel))!;

            return updateModel.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<VaultItem>().Logged(logger);
        }
    }

    public async Task<Result<IEnumerable<VaultItem>>> UpdateManyAsync(IEnumerable<VaultItem> vaultItems)
    {
        logger.LogInformation(nameof(UpdateManyAsync));

        try
        {
            IEnumerable<Guid> ids = vaultItems.Select(x => x.Id);
            IEnumerable<VaultItem> existingItems = await repository.GetManyAsync(new() { Where = x => ids.Contains(x.Id) });
            VaultItem[] notFoundItems = [.. vaultItems.Where(x => !existingItems.Select(y => y.Id).Contains(x.Id))];

            if (notFoundItems.Length > 0)
            {
                string message = $"{notFoundItems.Length} Vault Item IDs do not exist: [{string.Join(", ", notFoundItems.Select(x => x.Id))}]";
                return Result.Failed<IEnumerable<VaultItem>>(ResultFailureType.NotFound, message).Logged(logger);
            }

            Result validateResult = await validatorService.ValidateUpsertManyAsync(vaultItems);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue<IEnumerable<VaultItem>>().Logged(logger);

            IEnumerable<KeyValuePair<VaultItem, VaultItem>> updateModels = existingItems
                .Select(x => new KeyValuePair<VaultItem, VaultItem>(x, vaultItems.First(y => x.Id == y.Id) with { }));

            IEnumerable<VaultItem> updatedItems = await repository.UpdateManyAsync(updateModels);

            return updatedItems.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<VaultItem>>().Logged(logger);
        }
    }

    public async Task<Result<IEnumerable<VaultItem>>> UngroupManyAsync(IEnumerable<VaultItem> vaultItems)
    {
        logger.LogInformation(nameof(UngroupManyAsync));

        try
        {
            IEnumerable<Guid> ids = vaultItems.Where(x => x.GroupId.HasValue).Select(x => x.Id);
            IEnumerable<VaultItem> existingItems = await repository.GetManyAsync(new() { Where = x => ids.Contains(x.Id) });
            VaultItem[] notFoundItems = [.. vaultItems.Where(x => !existingItems.Select(y => y.Id).Contains(x.Id))];

            if (notFoundItems.Length > 0)
            {
                string message = $"{notFoundItems.Length} Vault Item IDs do not exist: [{string.Join(", ", notFoundItems.Select(x => x.Id))}]";
                return Result.Failed<IEnumerable<VaultItem>>(ResultFailureType.NotFound, message).Logged(logger);
            }

            IEnumerable<string> existingItemNames = existingItems.Select(x => x.Name);
            VaultItem[] conflictingUngroupedItems = [..await repository.GetManyAsync(new()
            {
                Where = x => x.GroupId == null && existingItemNames.Contains(x.Name)
            })];

            if (conflictingUngroupedItems.Length > 0)
            {
                string message = $"{conflictingUngroupedItems.Length} Vault Items have the same names as other existing ungrouped Vault Items:\n- {string.Join("\n- ", conflictingUngroupedItems.Select(x => $"\"{x.Name}\""))}";
                return Result.Failed<IEnumerable<VaultItem>>(ResultFailureType.Conflict, message).Logged(logger);
            }

            IEnumerable<KeyValuePair<VaultItem, VaultItem>> updateModels = existingItems
                .Select(x => new KeyValuePair<VaultItem, VaultItem>(x, vaultItems.First(y => x.Id == y.Id) with { GroupId = null }));

            IEnumerable<VaultItem> updatedItems = await repository.UpdateManyAsync(updateModels);

            return updatedItems.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<VaultItem>>().Logged(logger);
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
