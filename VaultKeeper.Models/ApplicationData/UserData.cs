namespace VaultKeeper.Models.ApplicationData;

public record UserData : AppData
{
    public string? MainPassword { get; set; }
    public string? CustomEntitiesDataPath { get; init; }
}
