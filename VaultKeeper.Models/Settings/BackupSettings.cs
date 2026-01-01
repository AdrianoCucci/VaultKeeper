namespace VaultKeeper.Models.Settings;

public record BackupSettings
{
    public static BackupSettings Default => new()
    {
        BackupDirectory = string.Empty,
        MaxBackups = 3,
        AutoBackupOnShutdown = false
    };

    public required string BackupDirectory { get; set; }
    public int MaxBackups { get; set; }
    public bool AutoBackupOnShutdown { get; set; }
}