namespace VaultKeeper.Models.ApplicationData;

public record UserData
{
    public string? MainPasswordHash { get; init; }
    public string? CustomEntitiesDataPath { get; init; }
}
