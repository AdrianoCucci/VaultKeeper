using System.Collections.Generic;

namespace VaultKeeper.Models;

public record SelectedItems
{
    public static SelectedItems Empty() => new();

    public static SelectedItems<T> Empty<T>() => new();

    public int Count { get; set; }
    public long TotalCount { get; set; }
}

public record SelectedItems<T> : SelectedItems
{
    public IList<T> Items { get; set; } = [];
    public int? LastSelectedItemIndex { get; set; }
}
