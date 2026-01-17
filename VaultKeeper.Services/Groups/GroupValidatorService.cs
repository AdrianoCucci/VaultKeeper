using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.Common.Exceptions;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions.Groups;

namespace VaultKeeper.Services.Groups;

public class GroupValidatorService(
    IRepository<Group> repository,
    IRepository<VaultItem> vaultItemRepository,
    ILogger<GroupValidatorService> logger) : IGroupValidatorService
{
    public async Task<Result> ValidateUpsertAsync(Group group)
    {
        logger.LogInformation(nameof(ValidateUpsertAsync));

        Result result = await ValidateUpsertInternalAsync(group);

        return result.Logged(logger);
    }

    public async Task<Result> ValidateUpsertManyAsync(IEnumerable<Group> groups)
    {
        logger.LogInformation(nameof(ValidateUpsertManyAsync));

        HashSet<string> duplicateNameConflicts = [.. groups.DuplicatesBy(x => x.Name).Select(x => x.Name)];
        if (duplicateNameConflicts.Count > 0)
        {
            string message = $"{duplicateNameConflicts.Count} Groups have duplicate names:\n- {string.Join("\n- ", duplicateNameConflicts.Select(x => $"\"{x}\""))}";
            return Result.Failed(ResultFailureType.Conflict, message);
        }

        Result[] results = await Task.WhenAll(groups.Select(ValidateUpsertInternalAsync));
        if (results.Any(x => !x.IsSuccessful))
            return results.ToAggregatedResult().Logged(logger);

        return Result.Ok().Logged(logger);
    }

    public async Task<Result> ValidateDeleteAsync(Group group, CascadeDeleteMode cascadeDeleteMode)
    {
        logger.LogInformation(nameof(ValidateDeleteAsync));

        Result result = await ValidateDeleteInternalAsync(group, cascadeDeleteMode);

        return result.Logged(logger);
    }

    private async Task<Result> ValidateUpsertInternalAsync(Group group)
    {
        if (string.IsNullOrWhiteSpace(group.Name))
            return Result.Failed(ResultFailureType.BadRequest, "Name is required.");

        bool isDuplicate = await repository.HasAnyAsync(new()
        {
            Where = x =>
                group.Name == x.Name &&
                group.Id != x.Id
        });

        if (isDuplicate)
            return Result.Failed<Group>(ResultFailureType.Conflict, $"Another Group named \"{group.Name}\" already exists.");

        return Result.Ok();
    }

    private async Task<Result> ValidateDeleteInternalAsync(Group group, CascadeDeleteMode cascadeDeleteMode)
    {
        bool exists = await repository.HasAnyAsync(new() { Where = x => x.Id == group.Id });
        if (!exists)
            return Result.Failed(ResultFailureType.NotFound, $"Group ID does not exist: ({group.Id})");

        if (cascadeDeleteMode == CascadeDeleteMode.OrphanChildren)
        {
            IEnumerable<VaultItem> childItems = await vaultItemRepository.GetManyAsync(new() { Where = x => x.GroupId == group.Id });
            IEnumerable<string> childItemNames = childItems.Select(x => x.Name);

            var ungroupedDuplicateItems = await vaultItemRepository.GetManyAsync(new() { Where = x => x.GroupId == null && childItemNames.Contains(x.Name) });
            var ungroupedDuplicateItemNames = ungroupedDuplicateItems.Select(x => x.Name);

            VaultItem[] conflictingChildren = [.. childItems.Where(x => ungroupedDuplicateItemNames.Contains(x.Name))];

            if (conflictingChildren.Length > 0)
            {
                string message = $"{conflictingChildren.Length} child {nameof(VaultItem)}s cannot be ungrouped as their names conflict with existing ungrouped {nameof(VaultItem)}s.";
                CascadeDeleteException<Group, VaultItem> exception = new(message)
                {
                    Parent = group,
                    ConflictingChildren = conflictingChildren
                };

                return Result.Failed(ResultFailureType.Conflict, message, exception);
            }
        }

        return Result.Ok();
    }
}