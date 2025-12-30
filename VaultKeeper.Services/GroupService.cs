using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.Groups.Extensions;
using VaultKeeper.Repositories.Abstractions;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class GroupService(IRepository<Group> repository, ILogger<GroupService> logger) : IGroupService
{
    public async Task<Result<IEnumerable<Group>>> GetManyAsync(ReadQuery<Group>? query = null)
    {
        logger.LogInformation(nameof(GetManyAsync));

        try
        {
            var items = await repository.GetManyAsync(query);
            return items.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<IEnumerable<Group>>().Logged(logger);
        }
    }

    public async Task<Result<Group>> AddAsync(NewGroup group)
    {
        logger.LogInformation(nameof(AddAsync));

        try
        {
            Group model = group.ToGroup() with { Id = Guid.NewGuid() };

            Result validateResult = await ValidateUpsertAsync(model);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue(model).Logged(logger);

            model = await repository.AddAsync(model);

            return model.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<Group>().Logged(logger);
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

            Result validateResult = await ValidateUpsertAsync(group);
            if (!validateResult.IsSuccessful)
                return validateResult.WithValue(group).Logged(logger);

            Group updateModel = group with { };

            updateModel = (await repository.UpdateAsync(existingModel, updateModel))!;

            return updateModel.ToOkResult().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<Group>().Logged(logger);
        }
    }

    public async Task<Result> DeleteAsync(Group group)
    {
        logger.LogInformation(nameof(DeleteAsync));

        try
        {
            bool didRemove = await repository.RemoveAsync(group);
            if (!didRemove)
                return Result.Failed<Group>(ResultFailureType.NotFound, $"Group ID does not exist: ({group.Id})").Logged(logger);

            return Result.Ok($"Group deleted successfuly (ID: {group.Id}).").Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    private async Task<Result> ValidateUpsertAsync(Group group)
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
}
