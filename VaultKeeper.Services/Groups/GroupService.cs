using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.Groups.Extensions;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions.Groups;

namespace VaultKeeper.Services.Groups;

public class GroupService(
    IRepository<Group> repository,
    IRepository<VaultItem> vaultItemRepository,
    IGroupValidatorService validatorService,
    ILogger<GroupService> logger) : IGroupService
{
    public async Task<Result<CountedData<Group>>> GetManyCountedAsync(ReadQuery<Group>? query = null)
    {
        logger.LogInformation(nameof(GetManyCountedAsync));

        try
        {
            CountedData<Group> items = await repository.GetManyCountedAsync(query);
            return items.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<CountedData<Group>>().Logged(logger);
        }
    }

    public async Task<Result<Group>> AddAsync(NewGroup group)
    {
        logger.LogInformation(nameof(AddAsync));

        try
        {
            Group model = group.ToGroup() with { Id = Guid.NewGuid() };

            Result validateResult = await validatorService.ValidateUpsertAsync(model);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue<Group>().Logged(logger);

            model = await repository.AddAsync(model);

            return model.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<Group>().Logged(logger);
        }
    }

    public async Task<Result<IEnumerable<Group>>> AddManyAsync(IEnumerable<NewGroup> groups)
    {
        logger.LogInformation(nameof(AddManyAsync));

        try
        {
            Group[] models = [.. groups.Select(x => x.ToGroup() with { Id = Guid.NewGuid() })];

            Result validateResult = await validatorService.ValidateUpsertManyAsync(models);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue<IEnumerable<Group>>().Logged(logger);

            models = [.. await repository.AddManyAsync(models)];

            return Result.Ok<IEnumerable<Group>>(models).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<Group>>().Logged(logger);
        }
    }

    public async Task<Result<Group>> UpdateAsync(Group group)
    {
        logger.LogInformation(nameof(UpdateAsync));

        try
        {
            Group? existingModel = await repository.GetFirstOrDefaultAsync(new()
            {
                Where = x => x.Id == group.Id
            });

            if (existingModel == null)
                return Result.Failed<Group>(ResultFailureType.NotFound, $"Vault Item ID does not exist: ({group.Id})").Logged(logger);

            Result validateResult = await validatorService.ValidateUpsertAsync(group);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue<Group>().Logged(logger);

            Group updateModel = group with { };

            updateModel = (await repository.UpdateAsync(existingModel, updateModel))!;

            return updateModel.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<Group>().Logged(logger);
        }
    }

    public async Task<Result> DeleteAsync(Group group, CascadeDeleteMode cascadeDeleteMode = CascadeDeleteMode.DeleteChildren)
    {
        logger.LogInformation(nameof(DeleteAsync));

        try
        {
            Result validateResult = await validatorService.ValidateDeleteAsync(group, cascadeDeleteMode);
            if (!validateResult.IsSuccessful)
                return validateResult.Logged(logger);

            _ = await repository.RemoveAsync(group);

            IEnumerable<VaultItem> childItems = await vaultItemRepository.GetManyAsync(new() { Where = x => x.GroupId == group.Id });

            if (cascadeDeleteMode == CascadeDeleteMode.OrphanChildren)
            {
                IEnumerable<KeyValuePair<VaultItem, VaultItem>> updateRequests = childItems.Select(x => new KeyValuePair<VaultItem, VaultItem>(x, x with { GroupId = null }));
                await vaultItemRepository.UpdateManyAsync(updateRequests);
            }
            else
            {
                await vaultItemRepository.RemoveManyAsync(childItems);
            }

            return Result.Ok($"Group deleted successfuly (ID: {group.Id}).").Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public async Task<Result<long>> DeleteAllEmptyAsync()
    {
        logger.LogInformation(nameof(DeleteAllEmptyAsync));

        try
        {
            IEnumerable<Group> groups = await repository.GetManyAsync();
            IEnumerable<VaultItem> vaultItems = await vaultItemRepository.GetManyAsync();
            IEnumerable<Guid> vaultItemGroupIds = vaultItems.Select(x => x.GroupId.GetValueOrDefault()).Distinct();

            Group[] emptyGroups = [.. groups.Where(x => !vaultItemGroupIds.Contains(x.Id))];
            if (emptyGroups.Length < 1)
                return Result.Ok<long>(0, "Zero empty Groups found - nothing to delete.").Logged(logger);

            long deleteCount = await repository.RemoveManyAsync(emptyGroups);

            return Result.Ok(deleteCount, $"{deleteCount} empty Groups deleted successfully.").Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<long>().Logged(logger);
        }
    }
}
