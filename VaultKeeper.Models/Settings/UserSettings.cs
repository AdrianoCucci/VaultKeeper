namespace VaultKeeper.Models.Settings;

public record UserSettings
{
    public AppThemeType AppTheme { get; set; }
    public BackupSettings? Backup { get; set; }
    public KeyGenerationSettings? KeyGeneration { get; set; }
}