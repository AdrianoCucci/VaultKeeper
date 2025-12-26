namespace VaultKeeper.Models.VaultItems.Extensions;

public static class VaultItemExtensions
{
    public static VaultItem ToVaultItem(this NewVaultItem newVaultItem) => new()
    {
        Name = newVaultItem.Name,
        Value = newVaultItem.Value,
        GroupId = newVaultItem.GroupId
    };

    public static NewVaultItem ToNewVaultItem(this VaultItem vaultItem) => new()
    {
        Name = vaultItem.Name,
        Value = vaultItem.Value,
        GroupId = vaultItem.GroupId
    };
}
