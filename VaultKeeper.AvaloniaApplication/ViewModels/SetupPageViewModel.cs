using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Errors;
using VaultKeeper.Models.Navigation.Extensions;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class SetupPageViewModel : ViewModelBase
{
    public SetPasswordFormViewModel SetPasswordFormVM { get; } = new();

    [ObservableProperty]
    private bool _canNavigateBack = false;

    private readonly IUserDataService _userDataService;
    private readonly IBackupService _backupService;
    private readonly IErrorReportingService _errorReportingService;

    public SetupPageViewModel(
        IUserDataService userDataService,
        IBackupService backupService,
        IErrorReportingService errorReportingService,
        INavigatorFactory? navFactory = null)
    {
        _userDataService = userDataService;
        _backupService = backupService;
        _errorReportingService = errorReportingService;

        INavigator? navigator = navFactory?.GetNavigator(nameof(MainWindowViewModel));
        if (navigator != null)
            _canNavigateBack = navigator.CurrentRoute.GetParamOrDefault<bool>(nameof(CanNavigateBack));
    }

    public async Task<bool> ProcessFormSubmissionAsync(SetPasswordForm form)
    {
        Result setPasswordResult = await _userDataService.SetMainPasswordAsync(form.Password!);
        bool isSuccessful = setPasswordResult.IsSuccessful;

        if (!isSuccessful)
        {
            _errorReportingService.ReportError(new()
            {
                Header = "Failed to Set Password",
                Message = $"({setPasswordResult.FailureType}) - {setPasswordResult.Message}",
                Severity = ErrorSeverity.Critical
            });
        }

        return isSuccessful;
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
}
