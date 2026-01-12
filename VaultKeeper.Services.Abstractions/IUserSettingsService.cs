using System.Collections.Generic;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.Services.Abstractions;

public interface IUserSettingsService
{
    UserSettings? GetUserSettings();
    UserSettings GetDefaultUserSettings();
    UserSettings GetUserSettingsOrDefault();
    UserSettings RestoreDefaultSettings();
    UserSettings SetUserSettings(UserSettings value);
    UserSettings SetAppThemeSettings(AppThemeSettings value);
    UserSettings SetBackupSettings(BackupSettings value);
    UserSettings SetKeyGenerationSettings(KeyGenerationSettings value);
    IEnumerable<EmptyGroupMode> GetEmptyGroupModeOptions();
    UserSettings SetEmptyGroupMode(EmptyGroupMode value);
}