using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.AvaloniaApplication.Abstractions;

public interface IBackupService
{
    Task<Result<BackupData?>> SaveBackupAsync(BackupSettings? backupSettings = null);
    Task<Result<BackupData?>> LoadBackupAsync(string backupPath);
    Task<Result<BackupData?>> LoadBackupFromFilePickerAsync();
}