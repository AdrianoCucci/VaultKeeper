namespace VaultKeeper.Models.VaultItems.Extensions;

public static class VaultItemExtensions
{
    public static VaultItem ToVaultItem(this NewVaultItem newVaultItem) => new()
    {
        Name = newVaultItem.Name,
        Value = newVaultItem.Value,
        GroupId = newVaultItem.GroupId
    };
}
