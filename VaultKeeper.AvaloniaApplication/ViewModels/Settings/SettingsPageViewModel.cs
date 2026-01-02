using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.Models;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class SettingsPageViewModel(
    IUserSettingsService settingsService,
    IPlatformService platformService,
    IBackupService backupService,
    IThemeService themeService,
    ICharSetService charSetService,
    IAppSessionService appSessionService) : ViewModelBase
{
    public UserSettings Model { get; private set; } = UserSettings.Default;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsBackupDirectoryInvalid), nameof(DoesBackupDirectoryExist))]
    private string _backupDirectory = string.Empty;

    [ObservableProperty]
    private int _maxBackups = 1;

    [ObservableProperty]
    private bool _autoBackupOnShutdown = false;

    [ObservableProperty]
    private IEnumerable<AppThemeDefinition> _themeDefinitions = [];

    [ObservableProperty]
    private AppThemeDefinition? _currentThemeDefinition;

    [ObservableProperty]
    private int _fontSize = 14;

    [ObservableProperty]
    private IEnumerable<CharSet> _charSets = [];

    [ObservableProperty]
    private CharSet _currentCharSet = CharSet.Default;

    private int _keyGenMinLength = 1;
    public int KeyGenMinLength { get => _keyGenMinLength; set => SetProperty(ref _keyGenMinLength, Math.Clamp(value, 1, _keyGenMaxLength)); }

    private int _keyGenMaxLength = 1;
    public int KeyGenMaxLength
    {
        get => _keyGenMaxLength;
        set
        {
            SetProperty(ref _keyGenMaxLength, value);
            if (value < _keyGenMinLength)
                KeyGenMinLength = value;
        }
    }

    public bool IsBackupDirectoryInvalid => Path.GetInvalidPathChars().Any(BackupDirectory.Contains);
    public bool DoesBackupDirectoryExist => Directory.Exists(BackupDirectory);

    private bool _isLoadingSavedSettings = false;

    public virtual void LoadSavedSettings()
    {
        _isLoadingSavedSettings = true;

        Model = settingsService?.GetUserSettingsOrDefault() ?? UserSettings.Default;

        BackupSettings backupSettings = Model.Backup ?? BackupSettings.Default;
        BackupDirectory = backupSettings.BackupDirectory;
        MaxBackups = backupSettings.MaxBackups;
        AutoBackupOnShutdown = backupSettings.AutoBackupOnShutdown;

        AppThemeSettings themeSettings = Model.Theme ?? AppThemeSettings.Default;
        ThemeDefinitions = themeService?.GetThemeDefinitions() ?? [];
        CurrentThemeDefinition = ThemeDefinitions.FirstOrDefault(x => x.ThemeType == themeSettings.ThemeType);
        FontSize = themeSettings.FontSize;

        KeyGenerationSettings keyGenerationSettings = Model.KeyGeneration ?? KeyGenerationSettings.Default;
        CharSets = charSetService?.GetCharSets() ?? [];
        CurrentCharSet = CharSets.FirstOrDefault(x => x.Type == keyGenerationSettings.CharSet?.Type) ?? CharSet.Default;
        KeyGenMaxLength = keyGenerationSettings.MaxLength;
        KeyGenMinLength = keyGenerationSettings.MinLength;

        UpdateServices(Model);

        _isLoadingSavedSettings = false;
    }

    public void SaveSettings()
    {
        Model = CreateUserSettings();
        UpdateServices(Model);
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
        BackupSettings backupSettings = CreateBackupSettings();
        Result<BackupData?> backupResult = await backupService.SaveBackupAsync(backupSettings);
        if (!backupResult.IsSuccessful)
        {
            // TODO: Handle error.
            return;
        }
    }

    public async Task LoadBackupAsync()
    {
        var loadResult = await backupService.LoadBackupFromFilePickerAsync();
        if (!loadResult.IsSuccessful)
        {
            // TODO: Handle error.
            return;
        }

        await appSessionService.LogoutAsync((nameof(MainWindowViewModel), nameof(LockScreenViewModel)));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!_isLoadingSavedSettings && e.PropertyName is not (nameof(IsBackupDirectoryInvalid)) or nameof(DoesBackupDirectoryExist))
            SaveSettings();
    }

    private void UpdateServices(UserSettings settings)
    {
        if (settingsService == null || themeService == null) return;

        settingsService.SetUserSettings(settings);

        AppThemeSettings themeSettings = settings.Theme ?? settingsService.GetDefaultUserSettings()?.Theme ?? AppThemeSettings.Default;
        themeService.SetTheme(themeSettings.ThemeType);
        themeService.SetBaseFontSize(themeSettings.FontSize);
    }

    private UserSettings CreateUserSettings() => new()
    {
        Theme = CreateThemeSettings(),
        Backup = CreateBackupSettings(),
        KeyGeneration = CreateKeyGenreationSettings()
    };

    private AppThemeSettings CreateThemeSettings() => new()
    {
        ThemeType = CurrentThemeDefinition?.ThemeType ?? Model.Theme?.ThemeType ?? AppThemeSettings.Default.ThemeType,
        FontSize = FontSize
    };

    private BackupSettings CreateBackupSettings() => new()
    {
        BackupDirectory = BackupDirectory,
        MaxBackups = MaxBackups,
        AutoBackupOnShutdown = AutoBackupOnShutdown
    };

    private KeyGenerationSettings CreateKeyGenreationSettings() => new()
    {
        CharSet = CurrentCharSet,
        MinLength = KeyGenMinLength,
        MaxLength = KeyGenMaxLength
    };
}
