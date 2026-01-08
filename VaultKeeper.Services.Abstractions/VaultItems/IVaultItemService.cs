using System.Collections.Generic;
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
    Task<Result<IEnumerable<VaultItem>>> UpdateManyAsync(IEnumerable<VaultItem> vaultItems);
    Task<Result<IEnumerable<VaultItem>>> UngroupManyAsync(IEnumerable<VaultItem> vaultItems);
    Task<Result> DeleteAsync(VaultItem vaultItem);
    Task<Result<long>> DeleteManyAsync(IEnumerable<VaultItem> vaultItems);
}