namespace VaultKeeper.Models.Settings;

public record BackupSettings
{
    public required string BackupDirectory { get; set; }
    public int MaxBackups { get; set; }
    public bool AutoBackupOnShutdown { get; set; }
}