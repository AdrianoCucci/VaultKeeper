using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.ApplicationData.Files;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.Services;

public class BackupService(
    ILogger<BackupService> logger,
    IAppDataService appDataService,
    IAppFileDefinitionService fileDefinitionService,
    IFileService fileService,
    IUserSettingsService userSettingsService,
    IPlatformService platformService) : IBackupService
{
    public Result CanCreateBackupAtDirectory(string directory)
    {
        logger.LogInformation($"{nameof(CanCreateBackupAtDirectory)} | directory: {{directory}}", directory);
        return fileService.CanWriteToDirectory(directory);
    }

    public async Task<Result<BackupData?>> SaveBackupAsync(BackupSettings? backupSettings = null)
    {
        logger.LogInformation(nameof(SaveBackupAsync));

        backupSettings ??= userSettingsService.GetDefaultUserSettings().Backup;
        Result<SavedData<BackupData>?> result = await appDataService.SaveBackupAsync(backupSettings);

        return result.WithValue<BackupData?>(result.Value?.Data).Logged(logger);
    }

    public async Task<Result<BackupData?>> LoadBackupAsync(string backupPath)
    {
        logger.LogInformation($"{nameof(LoadBackupAsync)} | path: {{backupPath}}", backupPath);

        Result<SavedData<BackupData>?> result = await appDataService.LoadBackupAsync(backupPath);

        return result.WithValue<BackupData?>(result.Value?.Data).Logged(logger);
    }

    public async Task<Result<BackupData?>> LoadBackupFromFilePickerAsync()
    {
        logger.LogInformation(nameof(LoadBackupFromFilePickerAsync));

        AppFileDefinition backupFileDefinition = fileDefinitionService.GetFileDefinitionByType(AppFileType.Backup);

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
            return Result.Ok<BackupData?>(null, "User cancelled file selection.").Logged(logger);

        string? filePath = files[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(filePath))
            return Result.Failed<BackupData?>(ResultFailureType.BadRequest, "File does not have a valid local path.").Logged(logger);

        Result<SavedData<BackupData>?> result = await appDataService.LoadBackupAsync(filePath);

        return result.WithValue<BackupData?>(result.Value?.Data).Logged(logger);
    }
}
