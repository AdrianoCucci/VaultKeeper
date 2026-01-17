namespace VaultKeeper.Models.ApplicationData;

public record AppConfigData
{
    public static AppConfigData Default => new();

    public string? EncryptionKeyPath { get; init; }
}
