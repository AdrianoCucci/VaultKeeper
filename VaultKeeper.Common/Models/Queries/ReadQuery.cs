using System;

namespace VaultKeeper.Common.Models.Queries;

public record ReadQuery<T>
{
    public Func<T, bool>? Where { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public SortQuery<T>? Sort { get; set; }
}
