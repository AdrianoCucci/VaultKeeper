using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Repositories.Abstractions;

namespace VaultKeeper.Repositories;

public class InMemoryRepository<T> : IRepository<T>
{
    private readonly HashSet<T> _items = [];

    public Task<IEnumerable<T>> GetManyAsync(ReadQuery<T>? query = null) =>
        Task.FromResult(_items.FromReadQuery(query));

    public Task<T?> GetFirstOrDefaultAsync(ReadQuery<T>? query = null, T? defaultValue = default) =>
        Task.FromResult(_items.FromReadQuery(query).FirstOrDefault(defaultValue));

    public Task<long> CountAsync(ReadQuery<T>? query = null) =>
        Task.FromResult(_items.FromReadQuery(query).LongCount());


    public Task<bool> HasAnyAsync(ReadQuery<T>? query = null) =>
        Task.FromResult(_items.FromReadQuery(query).Any());

    public Task<T> AddAsync(T item)
    {
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task<IEnumerable<T>> AddManyAsync(IEnumerable<T> items)
    {
        _items.UnionWith(items);
        return Task.FromResult(items);
    }

    public Task<T?> UpdateAsync(T originalItem, T newItem)
    {
        T? returnValue = default;

        if (_items.Remove(originalItem))
        {
            _items.Add(newItem);
            returnValue = newItem;
        }

        return Task.FromResult(returnValue);
    }

    public Task<IEnumerable<T>> UpdateManyAsync(IEnumerable<KeyValuePair<T, T>> items)
    {
        HashSet<T> updatedItems = [];

        foreach (var (originalItem, newItem) in items)
        {
            if (_items.Remove(originalItem))
            {
                _items.Add(newItem);
                updatedItems.Add(newItem);
            }
        }

        return Task.FromResult<IEnumerable<T>>(updatedItems);
    }

    public Task<bool> RemoveAsync(T item)
    {
        bool didRemove = _items.Remove(item);
        return Task.FromResult(didRemove);
    }

    public Task<long> RemoveManyAsync(IEnumerable<T> items)
    {
        long removeCount = 0;

        foreach (var item in items)
        {
            if (_items.Remove(item))
                removeCount++;
        }

        return Task.FromResult(removeCount);
    }

    public Task<long> RemoveAllAsync()
    {
        long count = _items.Count;
        _items.Clear();

        return Task.FromResult(count);
    }
}
