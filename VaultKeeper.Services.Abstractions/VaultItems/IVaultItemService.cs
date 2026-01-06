using System.Threading.Tasks;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.Services.Abstractions.VaultItems;

public interface IVaultItemService
{
    Task<Result<CountedData<VaultItem>>> GetManyCountedAsync(ReadQuery<VaultItem>? query = null);
    Task<Result<VaultItem>> AddAsync(NewVaultItem vaultItem, bool encrypt = false);
    Task<Result<VaultItem>> UpdateAsync(VaultItem vaultItem, bool encrypt = false);
    Task<Result> DeleteAsync(VaultItem vaultItem);
}