using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.Services.Abstractions.VaultItems;

public interface IVaultItemValidatorService
{
    Task<Result> ValidateUpsertAsync(VaultItem vaultItem);
    Task<Result> ValidateUpsertManyAsync(IEnumerable<VaultItem> vaultItems);
}