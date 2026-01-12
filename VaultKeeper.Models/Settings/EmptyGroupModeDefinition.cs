namespace VaultKeeper.Models.Settings;

public record EmptyGroupModeDefinition
{
    public EmptyGroupMode Mode { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string? Icon { get; init; }
}