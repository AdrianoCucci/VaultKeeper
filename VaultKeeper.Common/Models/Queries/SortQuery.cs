using System;

namespace VaultKeeper.Common.Models.Queries;

public record SortQuery<T>
{
    public required Func<T, object?> SortBy { get; init; }
    public SortDirection Direction { get; init; }
}
