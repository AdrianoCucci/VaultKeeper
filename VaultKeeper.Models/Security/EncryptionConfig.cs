namespace VaultKeeper.Models.Security;

public record EncryptionConfig
{
    public required string Key { get; init; }
    public required string IV { get; init; }
}
