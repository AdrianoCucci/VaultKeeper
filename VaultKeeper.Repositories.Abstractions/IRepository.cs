using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaultKeeper.Repositories.Abstractions;

public interface IRepository<T> : IReadOnlyRepository<T>
{
    Task<IEnumerable<T>> SetAllAsync(IEnumerable<T> items);

    Task<T> AddAsync(T item);

    Task<IEnumerable<T>> AddManyAsync(IEnumerable<T> items);

    Task<T?> UpdateAsync(T originalItem, T newItem);

    Task<IEnumerable<T>> UpdateManyAsync(IEnumerable<KeyValuePair<T, T>> items);

    Task<bool> RemoveAsync(T item);

    Task<long> RemoveManyAsync(IEnumerable<T> items);

    Task<long> RemoveAllAsync();
}