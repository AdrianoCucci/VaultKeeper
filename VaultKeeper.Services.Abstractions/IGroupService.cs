using System.Threading.Tasks;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.Services.Abstractions;

public interface IGroupService
{
    Task<Result<CountedData<Group>>> GetManyCountedAsync(ReadQuery<Group>? query = null);
    Task<Result<Group>> AddAsync(NewGroup group);
    Task<Result<Group>> UpdateAsync(Group group);
    Task<Result> DeleteAsync(Group group);
}