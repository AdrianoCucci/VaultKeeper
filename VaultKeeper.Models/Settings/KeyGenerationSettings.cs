namespace VaultKeeper.Models.Settings;

public record KeyGenerationSettings
{
    public required CharSet CharSet { get; set; }
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
}