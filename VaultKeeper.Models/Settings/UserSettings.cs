namespace VaultKeeper.Models.Settings;

public record UserSettings
{
    public static UserSettings Default => new()
    {
        Theme = AppThemeSettings.Default,
        Backup = BackupSettings.Default,
        KeyGeneration = KeyGenerationSettings.Default
    };

    public required AppThemeSettings Theme { get; set; }
    public required BackupSettings Backup { get; set; }
    public required KeyGenerationSettings KeyGeneration { get; set; }
}