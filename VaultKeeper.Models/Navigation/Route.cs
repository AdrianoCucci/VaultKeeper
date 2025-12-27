using System;

namespace VaultKeeper.Models.Navigation;

public record Route
{
    public required string Key { get; init; }
    public Func<object>? Content { get; init; }
    public Route[]? Children { get; init; }
}