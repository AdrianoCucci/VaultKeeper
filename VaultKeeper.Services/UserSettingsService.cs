using Microsoft.Extensions.Logging;
using System;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class UserSettingsService(ILogger<UserSettingsService> logger, ICache<UserData> userDataCache, ICharSetService charSetService) : IUserSettingsService
{
    private UserSettings DefaultSettings => new()
    {
        Theme = new()
        {
            ThemeType = AppThemeType.System,
            FontSize = 14
        },
        Backup = new()
        {
            BackupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace('\\', '/'),
            MaxBackups = 3
        },
        KeyGeneration = new()
        {
            CharSet = charSetService.GetDefaultCharSet(),
            MinLength = 32,
            MaxLength = 32
        }
    };

    public UserSettings? GetUserSettings()
    {
        logger.LogInformation(nameof(GetUserSettings));

        UserData? userData = userDataCache.Get();
        return userData?.Settings;
    }

    public UserSettings GetDefaultUserSettings()
    {
        logger.LogInformation(nameof(GetDefaultUserSettings));
        return DefaultSettings;
    }

    public UserSettings GetUserSettingsOrDefault()
    {
        logger.LogInformation(nameof(GetUserSettingsOrDefault));
        return GetUserSettings() ?? GetDefaultUserSettings();
    }

    public UserSettings RestoreDefaultSettings()
    {
        logger.LogInformation(nameof(RestoreDefaultSettings));
        return UpdateUserSettings(_ => DefaultSettings);
    }

    public UserSettings SetUserSettings(UserSettings value)
    {
        logger.LogInformation(nameof(SetUserSettings));
        return UpdateUserSettings(_ => value);
    }

    public UserSettings SetAppThemeSettings(AppThemeSettings value)
    {
        logger.LogInformation(nameof(SetAppThemeSettings));
        return UpdateUserSettings(settings => settings with { Theme = value });
    }

    public UserSettings SetBackupSettings(BackupSettings value)
    {
        logger.LogInformation(nameof(SetBackupSettings));
        return UpdateUserSettings(settings => settings with { Backup = value });
    }

    public UserSettings SetKeyGenerationSettings(KeyGenerationSettings value)
    {
        logger.LogInformation(nameof(SetKeyGenerationSettings));
        return UpdateUserSettings(settings => settings with { KeyGeneration = value });
    }

    private UserSettings UpdateUserSettings(Func<UserSettings, UserSettings> updateFunc)
    {
        UserData userData = userDataCache.Get() ?? new();
        UserSettings settings = GetUserSettingsOrDefault();

        userData = userData with { Settings = updateFunc.Invoke(settings) };
        userDataCache.Set(userData);

        return userData.Settings;
    }
}
