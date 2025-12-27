using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.Models.Navigation;

public record CurrentRoute
{
    public required string Key { get; init; }
    public required string ScopeKey { get; init; }
    public Dictionary<string, object?> Params { get; init; } = [];

    private readonly Func<object?>? _contentLazy;
    private object? _content;

    public object? Content
    {
        get
        {
            _content ??= _contentLazy?.Invoke();
            return _content;
        }
    }

    public CurrentRoute() { }

    [SetsRequiredMembers]
    public CurrentRoute(string routeGroupKey, Route other)
    {
        ScopeKey = routeGroupKey;
        Key = other.Key;
        _contentLazy = other.Content;
    }
}