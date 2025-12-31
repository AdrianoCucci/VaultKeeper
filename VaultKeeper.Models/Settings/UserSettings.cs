namespace VaultKeeper.Models.Settings;

public record UserSettings
{
    public AppThemeSettings? Theme { get; set; }
    public BackupSettings? Backup { get; set; }
    public KeyGenerationSettings? KeyGeneration { get; set; }
}