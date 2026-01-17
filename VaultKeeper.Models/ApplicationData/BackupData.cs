namespace VaultKeeper.Models.ApplicationData;

public record BackupData
{
    public required AppConfigData AppConfigData { get; init; }
    public required UserData UserData { get; init; }
    public required EntityData EntityData { get; init; }
}
