using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.Services.Abstractions;

public interface IVaultItemService
{
    Task<Result<IEnumerable<VaultItem>>> GetManyAsync(ReadQuery<VaultItem>? query = null);
    Task<Result<VaultItem>> AddAsync(NewVaultItem vaultItem);
}