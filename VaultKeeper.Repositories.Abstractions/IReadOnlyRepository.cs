using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Models.Queries;

namespace VaultKeeper.Repositories.Abstractions;

public interface IReadOnlyRepository<T>
{
    Task<IEnumerable<T>> GetManyAsync(ReadQuery<T>? query = null);

    Task<T?> GetFirstOrDefaultAsync(ReadQuery<T>? query = null, T? defaultValue = default);

    Task<long> CountAsync(ReadQuery<T>? query = null);

    Task<bool> HasAnyAsync(ReadQuery<T>? query = null);
}
