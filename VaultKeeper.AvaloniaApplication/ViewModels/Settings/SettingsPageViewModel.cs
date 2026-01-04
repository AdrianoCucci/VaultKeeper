using Avalonia.Media;
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

public partial class SettingsPageViewModel : ViewModelBase
{
    public UserSettings Model { get; private set; }
    public KeyGenerationSettingsViewModel KeyGenerationSettingsVM { get; init; }

    [ObservableProperty]
    private string _backupDirectory = string.Empty;

    [ObservableProperty]
    private int _maxBackups = 1;

    [ObservableProperty]
    private bool _autoBackupOnLogout = false;

    [ObservableProperty]
    private IEnumerable<AppThemeDefinition> _themeDefinitions = [];

    [ObservableProperty]
    private AppThemeDefinition? _currentThemeDefinition;

    [ObservableProperty]
    private int _fontSize = 14;


    private BackupDirectoryProperties _backupDirectoryProps;
    public BackupDirectoryProperties BackupDirectoryProps { get => _backupDirectoryProps; private set => SetProperty(ref _backupDirectoryProps, value); }

    private readonly IUserSettingsService? _userSettingsService;
    private readonly IPlatformService? _platformService;
    private readonly IBackupService? _backupService;
    private readonly IThemeService? _themeService;
    private readonly IAppSessionService? _appSessionService;

    private bool _isLoadingSavedSettings = false;

    public SettingsPageViewModel(
        IUserSettingsService userSettingsService,
        IPlatformService platformService,
        IBackupService backupService,
        IThemeService themeService,
        ICharSetService charSetService,
        IAppSessionService appSessionService)
    {
        _userSettingsService = userSettingsService;
        _platformService = platformService;
        _backupService = backupService;
        _themeService = themeService;
        _appSessionService = appSessionService;

        Model = userSettingsService?.GetDefaultUserSettings() ?? UserSettings.Default;
        KeyGenerationSettingsVM = new(charSetService, userSettingsService);
        KeyGenerationSettingsVM.PropertyChanged += KeyGenerationSettingsVM_PropertyChanged;
        _backupDirectoryProps = new(BackupDirectory, backupService);
    }

    public SettingsPageViewModel()
    {
        Model = UserSettings.Default;
        KeyGenerationSettingsVM = new();
        KeyGenerationSettingsVM.PropertyChanged += KeyGenerationSettingsVM_PropertyChanged;
        _backupDirectoryProps = new(BackupDirectory, null);
    }

    ~SettingsPageViewModel() => KeyGenerationSettingsVM.PropertyChanged -= KeyGenerationSettingsVM_PropertyChanged;

    public virtual void LoadSavedSettings()
    {
        _isLoadingSavedSettings = true;

        Model = _userSettingsService?.GetUserSettingsOrDefault() ?? UserSettings.Default;

        BackupSettings backupSettings = Model.Backup ?? BackupSettings.Default;
        BackupDirectory = backupSettings.BackupDirectory;
        MaxBackups = backupSettings.MaxBackups;
        AutoBackupOnLogout = backupSettings.AutoBackupOnLogout;

        AppThemeSettings themeSettings = Model.Theme ?? AppThemeSettings.Default;
        ThemeDefinitions = _themeService?.GetThemeDefinitions() ?? [];
        CurrentThemeDefinition = ThemeDefinitions.FirstOrDefault(x => x.ThemeType == themeSettings.ThemeType) ?? ThemeDefinitions.FirstOrDefault();
        FontSize = themeSettings.FontSize;

        KeyGenerationSettings keyGenerationSettings = Model.KeyGeneration ?? KeyGenerationSettings.Default;
        KeyGenerationSettingsVM.ApplySettings(keyGenerationSettings);

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
        if (_userSettingsService == null)
            return;

        _userSettingsService.RestoreDefaultSettings();
        LoadSavedSettings();
    }

    public async Task SetBackupDirectoryFromFolderPickerAsync()
    {
        if (_platformService == null) return;

        IReadOnlyList<IStorageFolder> storageFolders = await _platformService.OpenFolderPickerAsync(new()
        {
            Title = "Select Backup Directory"
        });

        if (storageFolders.Count < 1)
            return;

        var selectedFolderUri = storageFolders[0].Path;
        BackupDirectory = (selectedFolderUri.IsAbsoluteUri ? selectedFolderUri.LocalPath : selectedFolderUri.OriginalString).Replace('\\', '/');
    }

    public async Task CreateBackupAsync()
    {
        if (_backupService == null) return;

        BackupSettings backupSettings = CreateBackupSettings();
        Result<BackupData?> backupResult = await _backupService.SaveBackupAsync(backupSettings);
        if (!backupResult.IsSuccessful)
        {
            // TODO: Handle error.
            return;
        }

        // Refresh backup directory props.
        BackupDirectoryProps = new(BackupDirectory, _backupService);
    }

    public async Task LoadBackupAsync()
    {
        if (_backupService == null || _appSessionService == null) return;

        Result<BackupData?> loadResult = await _backupService.LoadBackupFromFilePickerAsync();
        if (!loadResult.IsSuccessful)
        {
            // TODO: Handle error.
            return;
        }

        if (loadResult.Value == null)
            return;

        UserSettings? settings = loadResult.Value?.UserData?.Settings;
        if (settings != null)
            UpdateServices(settings);

        await _appSessionService.LogoutAsync((nameof(MainWindowViewModel), nameof(LockScreenPageViewModel)));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(BackupDirectory))
            BackupDirectoryProps = new(BackupDirectory, _backupService);

        if (!_isLoadingSavedSettings)
            SaveSettings();
    }

    private void UpdateServices(UserSettings settings)
    {
        if (_userSettingsService == null || _themeService == null) return;

        _userSettingsService.SetUserSettings(settings);

        AppThemeSettings themeSettings = settings.Theme ?? _userSettingsService.GetDefaultUserSettings()?.Theme ?? AppThemeSettings.Default;
        _themeService.SetTheme(themeSettings.ThemeType);
        _themeService.SetBaseFontSize(themeSettings.FontSize);
    }

    private UserSettings CreateUserSettings() => new()
    {
        Theme = CreateThemeSettings(),
        Backup = CreateBackupSettings(),
        KeyGeneration = KeyGenerationSettingsVM.GetUpdatedModel()
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
        AutoBackupOnLogout = AutoBackupOnLogout
    };

    private void KeyGenerationSettingsVM_PropertyChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(e);

    public class BackupDirectoryProperties
    {
        public string? BackupDirectory { get; }
        public Uri? DirectoryUri { get; }
        public bool IsValid { get; }
        public bool DoesExist { get; }
        public string? MessageText { get; }
        public IBrush? MessageBrush { get; }

        public BackupDirectoryProperties(string? backupDirectory, IBackupService? backupService)
        {
            BackupDirectory = backupDirectory;
            if (!string.IsNullOrWhiteSpace(backupDirectory) && Uri.TryCreate(backupDirectory, UriKind.Absolute, out Uri? uri))
                DirectoryUri = uri;

            IsValid = DirectoryUri != null;
            DoesExist = Directory.Exists(backupDirectory);

            if (DirectoryUri == null)
            {
                MessageText = "Backup directory is invalid.";
                MessageBrush = new SolidColorBrush(Colors.Red);
            }
            else if (!DoesExist)
            {
                MessageText = $"Backup directory does not exist and will be created.";
                MessageBrush = new SolidColorBrush(Colors.Orange);
            }
            if (IsValid && DoesExist && backupService != null)
            {
                var canCreateBackupResult = backupService.CanCreateBackupAtDirectory(backupDirectory!);

                if (!canCreateBackupResult.IsSuccessful)
                {
                    MessageText = "Cannot create backup at this directory.";
                    MessageBrush = new SolidColorBrush(Colors.Red);
                    IsValid = false;
                }
            }
        }
    }
}
