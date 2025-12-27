namespace VaultKeeper.Models.ApplicationData;

public record UserData
{
    public string? MainPasswordHash { get; set; }
    public string? CustomEntitiesDataPath { get; init; }
}
