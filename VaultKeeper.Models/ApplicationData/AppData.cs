namespace VaultKeeper.Models.ApplicationData;

public abstract record AppData
{
    public required int Version { get; init; }
}
