using System;

namespace VaultKeeper.Models;

public record Group
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
}
