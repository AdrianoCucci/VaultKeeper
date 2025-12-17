using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class VaultItemService(IRepository<VaultItem> repository, ILogger<VaultItemService> logger) : IVaultItemService
{
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

    public async Task<Result<VaultItem>> AddAsync(NewVaultItem vaultItem)
    {
        logger.LogInformation(nameof(AddAsync));

        try
        {
            if (string.IsNullOrWhiteSpace(vaultItem.Name))
                return Result.Failed<VaultItem>(ResultFailureType.BadRequest, "Name is required.").Logged(logger);

            if (string.IsNullOrWhiteSpace(vaultItem.Value))
                return Result.Failed<VaultItem>(ResultFailureType.BadRequest, "Value is required.").Logged(logger);

            var isDuplicate = await repository.HasAnyAsync(new()
            {
                Where = x => x.Name == vaultItem.Name && x.GroupId != vaultItem.GroupId
            });

            if (isDuplicate)
                return Result.Failed<VaultItem>(ResultFailureType.Conflict, $"Another Vault Item named \"{vaultItem.Name}\" already exists.").Logged(logger);

            VaultItem addedItem = await repository.AddAsync(new()
            {
                Id = Guid.NewGuid(),
                Name = vaultItem.Name,
                Value = vaultItem.Value,
                GroupId = vaultItem.GroupId
            });

            return addedItem.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<VaultItem>().Logged(logger);
        }
    }
}
