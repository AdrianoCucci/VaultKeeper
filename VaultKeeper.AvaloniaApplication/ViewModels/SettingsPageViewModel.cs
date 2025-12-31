using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class SettingsPageViewModel(
    IUserSettingsService settingsService,
    IPlatformService platformService,
    IAppDataService appDataService,
    ICharSetService charSetService) : ViewModelBase
{
    [ObservableProperty]
    private UserSettings _model = settingsService?.GetUserSettingsOrDefault() ?? new();

    public UserSettings DefaultSettings => settingsService.GetDefaultUserSettings();

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
    }

    public async Task CreateBackupAsync()
    {
        string? backupDirectory = Model.Backup?.BackupDirectory;
        await appDataService.SaveBackupAsync(backupDirectory);
    }
}
