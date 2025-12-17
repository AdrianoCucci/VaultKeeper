using System.Collections.Generic;
using System.Linq;
using VaultKeeper.Common.Models.Queries;

namespace VaultKeeper.Common.Extensions;

public static class EnumerableExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
    {
        if (enumerable == null)
            return true;

        if (enumerable is ICollection<T> collection)
            return collection.Count < 1;

        return !enumerable.GetEnumerator().MoveNext();
    }

    public static IEnumerable<T> FromReadQuery<T>(this IEnumerable<T> enumerable, ReadQuery<T>? query)
    {
        if (query == null)
            return enumerable;

        if (query.Where != null)
            enumerable = enumerable.Where(query.Where);

        if (query.Skip.HasValue)
            enumerable = enumerable.Skip(query.Skip.Value);

        if (query.Take.HasValue)
            enumerable = enumerable.Skip(query.Take.Value);

        SortQuery<T>? sort = query.Sort;
        if (sort != null)
        {
            bool asc = sort.Direction == SortDirection.Ascending;
            enumerable = asc ? enumerable.OrderBy(sort.SortBy) : enumerable.OrderByDescending(sort.SortBy);
        }

        return enumerable;
    }
}
