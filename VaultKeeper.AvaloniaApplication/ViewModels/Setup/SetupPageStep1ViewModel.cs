using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Errors;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Setup;

public partial class SetupPageStep1ViewModel : ViewModelBase
{
    public SetPasswordFormViewModel SetPasswordFormVM { get; } = new();

    private readonly IAppDataService _appDataService;
    private readonly IAppConfigService _appConfigService;
    private readonly IUserDataService _userDataService;
    private readonly IBackupService _backupService;
    private readonly IErrorReportingService _errorReportingService;

    public SetupPageStep1ViewModel(
        IAppDataService appDataService,
        IAppConfigService appConfigService,
        IUserDataService userDataService,
        IBackupService backupService,
        IErrorReportingService errorReportingService)
    {
        _appDataService = appDataService;
        _appConfigService = appConfigService;
        _userDataService = userDataService;
        _backupService = backupService;
        _errorReportingService = errorReportingService;
    }

#if DEBUG
    public SetupPageStep1ViewModel()
    {
        _appDataService = null!;
        _appConfigService = null!;
        _userDataService = null!;
        _backupService = null!;
        _errorReportingService = null!;
    }
#endif

    public async Task<bool> ProcessFormSubmissionAsync(SetPasswordForm form)
    {
        _userDataService.ClearUserDataCache();

        Result setPasswordResult = await _userDataService.SetMainPasswordAsync(form.Password!);
        if (!setPasswordResult.IsSuccessful)
        {
            _errorReportingService.ReportError(new()
            {
                Header = "Failed to Set Password",
                Message = $"({setPasswordResult.FailureType}) - {setPasswordResult.Message}",
                Source = ErrorSource.Application,
                Severity = ErrorSeverity.Critical
            });
            return false;
        }

        // Delete any existing entity data to ensure clean setup for new user data.
        Result resetDataResult = await ResetUserDataAsync();
        if (!resetDataResult.IsSuccessful)
        {
            _errorReportingService.ReportError(new()
            {
                Header = "Failed to Initialize User Data",
                Message = $"({resetDataResult.FailureType}) - {resetDataResult.Message}",
                Source = ErrorSource.Application,
                Severity = ErrorSeverity.Critical
            });
            return false;
        }

        return true;
    }

    public async Task<bool> ImportBackupDataAsync()
    {
        Result<BackupData?> loadBackupResult = await _backupService.LoadBackupFromFilePickerAsync();
        if (!loadBackupResult.IsSuccessful)
        {
            _errorReportingService.ReportError(new()
            {
                Header = "Failed to Import Backup",
                Message = $"({loadBackupResult.FailureType}) - {loadBackupResult.Message}",
                Severity = ErrorSeverity.Critical
            });
        }

        return loadBackupResult.Value != null;
    }

    private async Task<Result> ResetUserDataAsync()
    {
        Result deleteEntityDataResult = await _appDataService.DeleteEntityDataAsync();
        if (!deleteEntityDataResult.IsSuccessful)
            return deleteEntityDataResult;

        Result<AppConfigData> removeCustomEncryptionKeyResult = await _appConfigService.SetEncryptionKeyFilePathAsync(null);

        return removeCustomEncryptionKeyResult;
    }
}
