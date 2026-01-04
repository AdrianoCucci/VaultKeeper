using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Forms;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Navigation.Extensions;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class SetupPageViewModel : ViewModelBase
{
    public SetupForm Form { get; } = new();

    [ObservableProperty]
    private bool _canNavigateBack = false;

    private readonly IAppDataService _appDataService;
    private readonly ISecurityService _securityService;
    private readonly IBackupService _backupService;

    public SetupPageViewModel(
        IAppDataService appDataService,
        ISecurityService securityService,
        IBackupService backupService,
        INavigatorFactory? navFactory = null)
    {
        _appDataService = appDataService;
        _securityService = securityService;
        _backupService = backupService;

        INavigator? navigator = navFactory?.GetNavigator(nameof(MainWindowViewModel));
        if (navigator != null)
            _canNavigateBack = navigator.CurrentRoute.GetParamOrDefault<bool>(nameof(CanNavigateBack));
    }

    public async Task<bool> SubmitFormAsync()
    {
        if (!Form.Validate()) return false;

        Result<string> hashPasswordResult = _securityService.CreateHash(Form.PasswordInput!);
        if (!hashPasswordResult.IsSuccessful)
            throw new Exception($"{nameof(SetupPageViewModel)}: Failed to submit form: {hashPasswordResult.Message}", hashPasswordResult.Exception);

        UserData userData = new()
        {
            UserId = Guid.NewGuid(),
            MainPasswordHash = hashPasswordResult.Value!
        };

        Result<SavedData<UserData>?> saveUserDataResult = await _appDataService.SaveUserDataAsync(userData, updateCaches: true);
        if (!saveUserDataResult.IsSuccessful)
            throw new Exception($"{nameof(SetupPageViewModel)}: Failed to submit form: {hashPasswordResult.Message}", saveUserDataResult.Exception);

        return true;
    }

    public async Task<bool> ImportBackupDataAsync()
    {
        Result<BackupData?> loadBackupResult = await _backupService.LoadBackupFromFilePickerAsync();
        if (!loadBackupResult.IsSuccessful)
            throw new Exception($"{nameof(SetupPageViewModel)}: Failed to load backup data: {loadBackupResult.Message}", loadBackupResult.Exception);

        return loadBackupResult.Value != null;
    }
}
