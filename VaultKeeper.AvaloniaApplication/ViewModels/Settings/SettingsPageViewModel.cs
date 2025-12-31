using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    IThemeService themeService) : ViewModelBase
{
    [ObservableProperty]
    private UserSettings _model = settingsService?.GetUserSettingsOrDefault() ?? new();

    [ObservableProperty]
    private IEnumerable<AppThemeDefinition> _themeDefinitions = themeService?.GetThemeDefinitions() ?? [];

    [ObservableProperty]
    private AppThemeDefinition? _currentThemeDefinition;

    [ObservableProperty]
    private int _currentFontSize = 14;

    [ObservableProperty]
    private bool _isBackupDirectoryInvalid = false;

    public UserSettings DefaultSettings => settingsService?.GetDefaultUserSettings() ?? new();

    public virtual void Initialize()
    {
        Model = settingsService?.GetUserSettingsOrDefault() ?? new();

        AppThemeSettings themeSettings = Model.Theme ?? DefaultSettings.Theme!;

        CurrentThemeDefinition = themeService?.GetThemeDefinitionByType(themeSettings.ThemeType);
        CurrentFontSize = themeSettings.FontSize;

        UpdateIsBackupDirectoryInvalid();
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
        var backupDirectory = selectedFolder.Path.AbsolutePath;

        BackupSettings settings = Model.Backup ?? DefaultSettings.Backup!;
        Model = settingsService.SetBackupSettings(settings with { BackupDirectory = backupDirectory });

        UpdateIsBackupDirectoryInvalid();
    }

    public void UpdateIsBackupDirectoryInvalid()
    {
        string? backupDirectory = Model.Backup?.BackupDirectory;
        IsBackupDirectoryInvalid = !Directory.Exists(backupDirectory);
    }

    public async Task CreateBackupAsync()
    {
        string? backupDirectory = Model.Backup?.BackupDirectory;
        await appDataService.SaveBackupAsync(backupDirectory);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(CurrentThemeDefinition):
                {
                    if (CurrentThemeDefinition != null)
                    {
                        AppThemeSettings themeSettings = (Model.Theme ?? DefaultSettings.Theme!) with { ThemeType = CurrentThemeDefinition.ThemeType };

                        settingsService.SetAppTheme(themeSettings);
                        themeService.SetTheme(themeSettings.ThemeType);
                    }
                    break;
                }
            case nameof(CurrentFontSize):
                {
                    AppThemeSettings themeSettings = (Model.Theme ?? DefaultSettings.Theme!) with { FontSize = CurrentFontSize };

                    settingsService.SetAppTheme(themeSettings);
                    themeService.SetBaseFontSize(themeSettings.FontSize);

                    break;
                }
        }

        if (e.PropertyName == nameof(CurrentThemeDefinition) && CurrentThemeDefinition != null)
        {
            AppThemeSettings themeSettings = (Model.Theme ?? DefaultSettings.Theme!) with { ThemeType = CurrentThemeDefinition.ThemeType };

            settingsService.SetAppTheme(themeSettings);
            themeService.SetTheme(themeSettings.ThemeType);
        }
    }
}
