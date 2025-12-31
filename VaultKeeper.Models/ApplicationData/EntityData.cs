using System;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.Models.ApplicationData;

public record EntityData
{
    public Guid UserId { get; init; }
    public VaultItem[]? VaultItems { get; init; }
    public Group[]? Groups { get; init; }
}
