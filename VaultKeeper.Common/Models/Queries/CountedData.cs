using System.Collections.Generic;

namespace VaultKeeper.Common.Models.Queries;

public record class CountedData<T>
{
    public IReadOnlyCollection<T> Items { get; init; }
    public long TotalCount { get; set; }
    public long ItemsCount { get; }

    public CountedData(IReadOnlyCollection<T> items, long totalCount)
    {
        Items = items;
        TotalCount = totalCount;
        ItemsCount = items.Count;
    }

    public CountedData(IEnumerable<T> items, long totalCount) : this([.. items], totalCount) { }

    public CountedData() : this([], 0) { }
}
