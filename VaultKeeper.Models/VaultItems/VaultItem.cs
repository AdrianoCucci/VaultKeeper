using System;

namespace VaultKeeper.Models.VaultItems;

public record VaultItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid? GroupId { get; set; }
}
