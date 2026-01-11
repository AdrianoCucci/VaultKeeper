using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions.VaultItems;

namespace VaultKeeper.Services.VaultItems;

public class VaultItemValidatorService(
    IRepository<VaultItem> repository,
    IRepository<Group> groupRepository,
    ILogger<VaultItemValidatorService> logger) : IVaultItemValidatorService
{
    public async Task<Result> ValidateUpsertAsync(VaultItem vaultItem)
    {
        logger.LogInformation(nameof(ValidateUpsertAsync));

        Result result = await ValidateUpsertInternalAsync(vaultItem);

        return result.Logged(logger);
    }

    public async Task<Result> ValidateUpsertManyAsync(IEnumerable<VaultItem> vaultItems)
    {
        logger.LogInformation(nameof(ValidateUpsertManyAsync));

        HashSet<string> duplicateNameConflicts = [.. vaultItems.DuplicatesBy(x => x.Name).Select(x => x.Name)];
        if (duplicateNameConflicts.Count > 0)
        {
            string message = $"{duplicateNameConflicts.Count} Vault Items have duplicate names:\n- {string.Join("\n- ", duplicateNameConflicts.Select(x => $"\"{x}\""))}";
            return Result.Failed(ResultFailureType.Conflict, message);
        }

        Result[] results = await Task.WhenAll(vaultItems.Select(ValidateUpsertInternalAsync));
        if (results.Any(x => !x.IsSuccessful))
            return results.ToAggregatedResult(ResultFailureType.BadRequest).Logged(logger);

        return Result.Ok().Logged(logger);
    }

    private async Task<Result> ValidateUpsertInternalAsync(VaultItem vaultItem)
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
        {
            var message = $"Another Vault Item named \"{vaultItem.Name}\" already exists";
            if (vaultItem.GroupId.HasValue)
                message += " in this group";

            return Result.Failed<VaultItem>(ResultFailureType.Conflict, $"{message}.");
        }

        if (vaultItem.GroupId.HasValue)
        {
            bool groupExists = await groupRepository.HasAnyAsync(new() { Where = x => x.Id == vaultItem.GroupId.Value });
            if (!groupExists)
                return Result.Failed(ResultFailureType.NotFound, $"Group ID does not exist ({vaultItem.GroupId.Value}).");
        }

        return Result.Ok();
    }
}