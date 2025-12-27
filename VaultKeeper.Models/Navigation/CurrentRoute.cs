using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.Models.Navigation;

public record CurrentRoute : Route
{
    public Dictionary<string, object?> Params { get; init; } = [];

    public CurrentRoute() { }

    [SetsRequiredMembers]
    public CurrentRoute(Route other)
    {
        Key = other.Key;
        Content = other.Content;
    }
}