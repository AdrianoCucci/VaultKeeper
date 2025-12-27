namespace VaultKeeper.Models.ApplicationData;

public record UserData
{
    public string? MainPassword { get; set; }
    public string? CustomEntitiesDataPath { get; init; }
}
