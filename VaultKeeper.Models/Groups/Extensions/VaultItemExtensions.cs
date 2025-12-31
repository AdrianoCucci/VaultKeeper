using System;

namespace VaultKeeper.Models.Groups.Extensions;

public static class GroupExtensions
{
    public static Group ToGroup(this NewGroup newVaultItem) => new()
    {
        Id = Guid.Empty,
        Name = newVaultItem.Name
    };

    public static NewGroup ToNewGroup(this Group vaultItem) => new()
    {
        Name = vaultItem.Name
    };
}
