using System;

namespace VaultKeeper.Models.VaultItems;

public record NewVaultItem
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public Guid? GroupId { get; set; }
}
