using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Models.ApplicationData.Files;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public class SetupViewModel(IAppDataService appDataService, IPlatformService platformService) : ViewModelBase
{
    public async Task ImportBackupDataAsync()
    {
        AppFileDefinition backupFileDefinition = appDataService.GetFileDefinition(AppFileType.Backup);

        IReadOnlyList<IStorageFile> files = await platformService.OpenFilePickerAsync(new()
        {
            Title = "Import Backup",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new(backupFileDefinition.Name)
                {
                    Patterns = [$"*{backupFileDefinition.Extension}"]
                }
            ]
        });

        if (files.Count < 1)
            return;

        // TODO: Process file import.
    }
}
