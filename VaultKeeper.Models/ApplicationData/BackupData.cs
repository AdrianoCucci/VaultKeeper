namespace VaultKeeper.Models.ApplicationData;

public record BackupData
{
    public required UserData UserData { get; init; }
    public required EntityData EntityData { get; init; }
}
