using System.Threading.Tasks;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.Services.Abstractions.Groups;

public interface IGroupValidatorService
{
    Task<Result> ValidateUpsertAsync(Group group);
    Task<Result> ValidateDeleteAsync(Group group, CascadeDeleteMode cascadeDeleteMode);
}