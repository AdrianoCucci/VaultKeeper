using VaultKeeper.Models.Settings;

namespace VaultKeeper.Services.Abstractions;

public interface IUserSettingsService
{
    UserSettings? GetUserSettings();
    UserSettings GetDefaultUserSettings();
    UserSettings GetUserSettingsOrDefault();
    UserSettings SetAppTheme(AppThemeType value);
    UserSettings SetBackupSettings(BackupSettings value);
    UserSettings SetKeyGenerationSettings(KeyGenerationSettings value);
}