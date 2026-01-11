using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public static IEnumerable<TSource> DuplicatesBy<TSource, TKey>(this IEnumerable<TSource>? enumerable, Func<TSource, TKey> keySelector)
    {
        if (enumerable == null)
            return [];

        return enumerable
            .GroupBy(keySelector)
            .Where(x => x.Count() > 1)
            .SelectMany(x => x);
    }

    public static IEnumerable<T> Duplicates<T>(this IEnumerable<T>? enumerable) => DuplicatesBy(enumerable, x => x);

    public static bool HasIndex<T>(this IList<T>? list, int index) => list != null && index.IsBetween(0, list.Count - 1);

    public static T? FirstOrDefaultByIndex<T>(this IList<T>? list, int index, T? defaultValue = default) =>
        HasIndex(list, index) ? list![index] : defaultValue;

    public static int FindIndex<T>(this IList<T> list, Func<T, bool> predicate)
    {
        T? item = list.FirstOrDefault(predicate);
        return item != null ? list.IndexOf(item) : -1;
    }

    public static ObservableCollection<T> AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }

        return collection;
    }

    public static ObservableCollection<T> RemoveRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Remove(item);
        }

        return collection;
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
