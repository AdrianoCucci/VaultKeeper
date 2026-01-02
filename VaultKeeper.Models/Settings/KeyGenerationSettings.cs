namespace VaultKeeper.Models.Settings;

public record KeyGenerationSettings
{
    public static KeyGenerationSettings Default => new()
    {
        CharSet = CharSet.Default,
        MinLength = 10,
        MaxLength = 10
    };

    public required CharSet CharSet { get; set; }
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
}