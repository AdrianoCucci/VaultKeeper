using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class UserSettingsService(ILogger<UserSettingsService> logger, ICache<UserSettings> userSettingsCache, ICharSetService charSetService) : IUserSettingsService
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
        },
        EmptyGroupMode = EmptyGroupMode.Keep
    };

    private IEnumerable<EmptyGroupModeDefinition> EmptyGroupModeDefinitions =>
    [
        new()
        {
            Mode = EmptyGroupMode.Keep,
            Name = "Keep",
            Description = "Keep Groups when all Keys are removed from them."
        },
        new()
        {
            Mode = EmptyGroupMode.Delete,
            Name = "Delete",
            Description = "Automatically delete Groups when all Keys are removed from them."
        }
    ];

    public UserSettings? GetUserSettings()
    {
        logger.LogInformation(nameof(GetUserSettings));
        return userSettingsCache.Get();
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

    public IEnumerable<EmptyGroupModeDefinition> GetEmptyGroupModeDefinitions()
    {
        logger.LogInformation(nameof(GetEmptyGroupModeDefinitions));
        return EmptyGroupModeDefinitions;
    }

    public UserSettings SetEmptyGroupMode(EmptyGroupMode value)
    {
        logger.LogInformation(nameof(SetEmptyGroupMode));
        return UpdateUserSettings(settings => settings with { EmptyGroupMode = value });
    }

    private UserSettings UpdateUserSettings(Func<UserSettings, UserSettings> updateFunc)
    {
        UserSettings settings = userSettingsCache.Get() ?? DefaultSettings;
        UserSettings updatedSettings = updateFunc.Invoke(settings);

        userSettingsCache.Set(updatedSettings);

        return settings;
    }
}
