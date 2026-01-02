namespace VaultKeeper.Models.Settings;

public record BackupSettings
{
    public static BackupSettings Default => new()
    {
        BackupDirectory = string.Empty,
        MaxBackups = 3,
        AutoBackupOnLogout = false
    };

    public required string BackupDirectory { get; set; }
    public int MaxBackups { get; set; }
    public bool AutoBackupOnLogout { get; set; }
}