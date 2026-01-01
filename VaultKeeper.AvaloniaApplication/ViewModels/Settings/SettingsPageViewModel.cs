using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.Models;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class SettingsPageViewModel(
    IUserSettingsService settingsService,
    IPlatformService platformService,
    IAppDataService appDataService,
    IThemeService themeService,
    ICharSetService charSetService) : ViewModelBase
{
    [ObservableProperty]
    private UserSettings _model = UserSettings.Default;

    private string _backupDirectory = string.Empty;
    public string BackupDirectory
    {
        get => _backupDirectory;
        set
        {
            SetProperty(ref _backupDirectory, value);
            Model.Backup = (Model.Backup ?? BackupSettings.Default) with { BackupDirectory = value };

            settingsService?.SetBackupSettings(Model.Backup);

            OnPropertyChanged(nameof(IsBackupDirectoryInvalid));
        }
    }

    private int _maxBackups = 1;
    public int MaxBackups
    {
        get => _maxBackups;
        set
        {
            SetProperty(ref _maxBackups, value);
            Model.Backup = (Model.Backup ?? BackupSettings.Default) with { MaxBackups = value };

            settingsService?.SetBackupSettings(Model.Backup);
        }
    }

    [ObservableProperty]
    private IEnumerable<AppThemeDefinition> _themeDefinitions = [];

    private AppThemeDefinition? _currentThemeDefinition;
    public AppThemeDefinition? CurrentThemeDefinition
    {
        get => _currentThemeDefinition;
        set
        {
            SetProperty(ref _currentThemeDefinition, value);

            if (value != null)
            {
                Model.Theme = (Model.Theme ?? AppThemeSettings.Default) with { ThemeType = value.ThemeType };

                settingsService?.SetAppThemeSettings(Model.Theme);
                themeService?.SetTheme(value.ThemeType);
            }
        }
    }

    private int _currentFontSize = 14;
    public int CurrentFontSize
    {
        get => _currentFontSize;
        set
        {
            SetProperty(ref _currentFontSize, value);
            Model.Theme = (Model.Theme ?? AppThemeSettings.Default) with { FontSize = value };

            settingsService?.SetAppThemeSettings(Model.Theme);
            themeService?.SetBaseFontSize(value);
        }
    }


    [ObservableProperty]
    private IEnumerable<CharSet> _charSets = [];

    private CharSet _currentCharSet = CharSet.Default;
    public CharSet CurrentCharSet
    {
        get => _currentCharSet;
        set
        {
            SetProperty(ref _currentCharSet, value);
            Model.KeyGeneration = (Model.KeyGeneration ?? KeyGenerationSettings.Default) with { CharSet = value };

            settingsService?.SetKeyGenerationSettings(Model.KeyGeneration);
        }
    }

    private int _keyGenMinLength = 1;
    public int KeyGenMinLength
    {
        get => _keyGenMinLength;
        set
        {
            SetProperty(ref _keyGenMinLength, Math.Clamp(value, 1, _keyGenMaxLength));
            Model.KeyGeneration = (Model.KeyGeneration ?? KeyGenerationSettings.Default) with { MinLength = value };

            settingsService?.SetKeyGenerationSettings(Model.KeyGeneration);
        }
    }

    private int _keyGenMaxLength = 1;
    public int KeyGenMaxLength
    {
        get => _keyGenMaxLength;
        set
        {
            SetProperty(ref _keyGenMaxLength, value);
            Model.KeyGeneration = (Model.KeyGeneration ?? KeyGenerationSettings.Default) with { MaxLength = value };

            settingsService?.SetKeyGenerationSettings(Model.KeyGeneration);

            if (value < _keyGenMinLength)
                KeyGenMinLength = value;
        }
    }

    public bool IsBackupDirectoryInvalid => !Directory.Exists(_backupDirectory);

    public virtual void LoadSavedSettings()
    {
        Model = settingsService?.GetUserSettingsOrDefault() ?? UserSettings.Default;
        
        BackupSettings backupSettings = Model.Backup ?? BackupSettings.Default;
        BackupDirectory = backupSettings.BackupDirectory;
        MaxBackups = backupSettings.MaxBackups;

        AppThemeSettings themeSettings = Model.Theme ?? AppThemeSettings.Default;
        ThemeDefinitions = themeService?.GetThemeDefinitions() ?? [];
        CurrentThemeDefinition = ThemeDefinitions.FirstOrDefault(x => x.ThemeType == themeSettings.ThemeType);
        CurrentFontSize = themeSettings.FontSize;

        KeyGenerationSettings keyGenerationSettings = Model.KeyGeneration ?? KeyGenerationSettings.Default;
        CharSets = charSetService?.GetCharSets() ?? [];
        CurrentCharSet = CharSets.FirstOrDefault(x => x.Type == keyGenerationSettings.CharSet?.Type) ?? CharSet.Default;
        KeyGenMaxLength = keyGenerationSettings.MaxLength;
        KeyGenMinLength = keyGenerationSettings.MinLength;
    }

    public void SaveSettings()
    {
        if (settingsService != null)
            Model = settingsService.SetUserSettings(Model);
    }

    public void RestoreDefaultSettings()
    {
        if (settingsService == null)
            return;

        settingsService.RestoreDefaultSettings();
        LoadSavedSettings();
    }

    public async Task SetBackupDirectoryFromFolderPickerAsync()
    {
        IReadOnlyList<IStorageFolder> storageFolders = await platformService.OpenFolderPickerAsync(new()
        {
            Title = "Select Backup Directory"
        });

        if (storageFolders.Count < 1)
            return;

        IStorageFolder selectedFolder = storageFolders[0];
        BackupDirectory = selectedFolder.Path.AbsolutePath;
    }

    public async Task CreateBackupAsync()
    {
        string? backupDirectory = Model.Backup?.BackupDirectory;
        await appDataService.SaveBackupAsync(backupDirectory);
    }
}
