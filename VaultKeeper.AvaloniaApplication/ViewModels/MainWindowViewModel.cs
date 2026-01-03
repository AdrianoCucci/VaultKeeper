using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Navigation;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? _content = null;

    [ObservableProperty]
    private bool _isInitializing = false;

    private readonly INavigator _navigator;
    private readonly IAppDataService _appDataService;
    private readonly IThemeService _themeService;

    public MainWindowViewModel(
        INavigatorFactory navFactory,
        IAppDataService appDataService,
        IThemeService themeService)
    {
        _navigator = navFactory.GetRequiredNavigator(nameof(MainWindowViewModel));
        _appDataService = appDataService;
        _themeService = themeService;
        _navigator.Navigated += Navigator_Navigated;
    }

    ~MainWindowViewModel() => _navigator.Navigated -= Navigator_Navigated;

    public async Task InitializeContentAsync()
    {
        try
        {
            IsInitializing = true;

            Result<SavedData<UserData>?> loadUserDataResult = await _appDataService.LoadUserDataAsync(updateCaches: true);
            if (!loadUserDataResult.IsSuccessful)
                throw new Exception($"{nameof(MainWindowViewModel)} failed to load user data: {loadUserDataResult.Message}", loadUserDataResult.Exception);

            SavedData<UserData>? userData = loadUserDataResult.Value;
            AppThemeSettings? themeSettings = userData?.Data.Settings?.Theme;

            if (themeSettings != null)
            {
                _themeService.SetTheme(themeSettings.ThemeType);
                _themeService.SetBaseFontSize(themeSettings.FontSize);
            }

            if (string.IsNullOrWhiteSpace(userData?.Data.MainPasswordHash))
                NavigateToSetup();
            else
                NavigateToLockscreen();
        }
        finally
        {
            IsInitializing = false;
        }
    }

    public void NavigateToSetup(bool canNavigateBack = false) => _navigator.Navigate(nameof(SetupPageViewModel), new()
    {
        { nameof(SetupPageViewModel.CanNavigateBack), canNavigateBack }
    });

    public void NavigateToLockscreen() => _navigator.Navigate(nameof(LockScreenPageViewModel));

    public void NavigateToHome() => _navigator.Navigate(nameof(HomeViewModel));

    private void Navigator_Navigated(object? sender, CurrentRoute e) => Content = e.Content;
}