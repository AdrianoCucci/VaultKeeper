using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.Models;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class SettingsPageViewModel : ViewModelBase
{
    public UserSettings Model { get; private set; }
    public EncryptionKeyFileViewModel EncryptionKeyFileVM { get; init; }
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

    [ObservableProperty]
    private IEnumerable<EmptyGroupModeDefinition> _emptyGroupModeDefinitions = [];

    [ObservableProperty]
    private EmptyGroupModeDefinition? _currentEmptyGroupModeDefinition;

    [ObservableProperty]
    private object? _overlayContent;

    [ObservableProperty]
    private bool _isOverlayVisible;

    private BackupDirectoryProperties _backupDirectoryProps;
    public BackupDirectoryProperties BackupDirectoryProps { get => _backupDirectoryProps; private set => SetProperty(ref _backupDirectoryProps, value); }

    private readonly IUserSettingsService? _userSettingsService;
    private readonly IPlatformService? _platformService;
    private readonly IBackupService? _backupService;
    private readonly IThemeService? _themeService;
    private readonly IAppSessionService? _appSessionService;
    private readonly IErrorReportingService? _errorReportingService;

    private bool _isLoadingSavedSettings = false;

    public SettingsPageViewModel(
        IUserSettingsService userSettingsService,
        IPlatformService platformService,
        IBackupService backupService,
        IThemeService themeService,
        IAppSessionService appSessionService,
        IErrorReportingService errorReportingService,
        IServiceProvider serviceProvider)
    {
        _userSettingsService = userSettingsService;
        _platformService = platformService;
        _backupService = backupService;
        _themeService = themeService;
        _appSessionService = appSessionService;
        _errorReportingService = errorReportingService;

        Model = userSettingsService?.GetDefaultUserSettings() ?? UserSettings.Default;

        EncryptionKeyFileVM = serviceProvider.GetRequiredService<EncryptionKeyFileViewModel>();

        KeyGenerationSettingsVM = serviceProvider.GetRequiredService<KeyGenerationSettingsViewModel>();
        KeyGenerationSettingsVM.PropertyChanged += KeyGenerationSettingsVM_PropertyChanged;

        _backupDirectoryProps = new(BackupDirectory, backupService);
    }

    public SettingsPageViewModel()
    {
        Model = UserSettings.Default;
        EncryptionKeyFileVM = new();
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

        EmptyGroupModeDefinitions = _userSettingsService?.GetEmptyGroupModeDefinitions() ?? [];
        CurrentEmptyGroupModeDefinition = EmptyGroupModeDefinitions.FirstOrDefault(x => x.Mode == Model.EmptyGroupMode);

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

        if (storageFolders.Count < 1) return;

        string? folderPath = storageFolders[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(folderPath)) return;

        BackupDirectory = folderPath.Replace('\\', '/');
    }

    public async Task CreateBackupAsync()
    {
        if (_backupService == null) return;

        BackupSettings backupSettings = CreateBackupSettings();
        Result<BackupData?> backupResult = await _backupService.SaveBackupAsync(backupSettings);
        if (!backupResult.IsSuccessful)
        {
            _errorReportingService?.ReportError(new()
            {
                Header = "Create Backup Failure",
                Message = backupResult.Message ?? "An unknown error occurred",
                Exception = backupResult.Exception
            });
            return;
        }

        // Refresh backup directory props.
        BackupDirectoryProps = new(BackupDirectory, _backupService);
        ShowOverlay(new PromptViewModel
        {
            Header = "Backup Successful",
            Message = $"Backup file was successfully created at:\n\"{backupSettings.BackupDirectory}\"",
            AckwnoledgedAction = HideOverlay
        });
    }

    public async Task LoadBackupAsync()
    {
        if (_backupService == null || _appSessionService == null) return;

        Result<BackupData?> loadResult = await _backupService.LoadBackupFromFilePickerAsync();
        if (!loadResult.IsSuccessful)
        {
            _errorReportingService?.ReportError(new()
            {
                Header = "Load Backup Failure",
                Message = loadResult.Message ?? "An unknown error occurred",
                Exception = loadResult.Exception
            });
            return;
        }

        if (loadResult.Value == null)
            return;

        UserSettings? settings = loadResult.Value?.UserData?.Settings;
        if (settings != null)
            UpdateServices(settings);

        await _appSessionService.LogoutAsync((nameof(MainWindowViewModel), nameof(LockScreenPageViewModel)));
    }

    public void GenerateEncryptionKeyFile() => ChangeEncryptionKeyFile(() => _ = EncryptionKeyFileVM.GenerateKeyFileAsync());

    public void SelectEncryptionKeyFile() => ChangeEncryptionKeyFile(() => _ = EncryptionKeyFileVM.SelectKeyFileAsync());

    public void PromptRemoveEncryptionKeyAsync() => ShowOverlay(new ConfirmPromptViewModel
    {
        Header = "Remove Encryption Key",
        Message = string.Join('\n',
        [
            "Are you sure you want to remove the reference to your encryption key file?",
            "The file will not be deleted, but Vault Keeper will revert back to using its built-in encryption key, and your saved data will be re-encrypted using this key.",
            "Any backups you have created using this key will no longer be restorable.",
            string.Empty,
            "It is highly recommended to create a backup first before continuing."
        ]),
        CancelAction = HideOverlay,
        ConfirmAction = async _ =>
        {
            if (await EncryptionKeyFileVM.RemoveEncryptionKeyReferenceAsync())
                HideOverlay();
        }
    });

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(BackupDirectory))
            BackupDirectoryProps = new(BackupDirectory, _backupService);

        if (!_isLoadingSavedSettings)
            SaveSettings();
    }

    private void ChangeEncryptionKeyFile(Action action)
    {
        if (!EncryptionKeyFileVM.HasExistingKey)
        {
            action.Invoke();
            return;
        }

        ShowOverlay(new PromptViewModel
        {
            Header = "Changing Your Encryption Key",
            Message = string.Join('\n',
            [
                "Note: Changing your encryption key will re-encrypt your saved data using the new key.",
                "Any backups you have created using the old encryption key will no longer be restorable.",
            ]),
            AckwnoledgedAction = () =>
            {
                HideOverlay();
                action.Invoke();
            }
        });
    }

    private void ShowOverlay(object content)
    {
        OverlayContent = content;
        IsOverlayVisible = true;
    }

    private void HideOverlay()
    {
        IsOverlayVisible = false;
        OverlayContent = null;
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
        KeyGeneration = KeyGenerationSettingsVM.GetUpdatedModel(),
        EmptyGroupMode = CurrentEmptyGroupModeDefinition?.Mode ?? Model.EmptyGroupMode
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
