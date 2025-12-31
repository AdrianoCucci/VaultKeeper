using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.Navigation;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Navigation;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;

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
    private readonly ICache<UserData> _userDataCache;

    public MainWindowViewModel(
        INavigatorFactory navFactory,
        IAppDataService appDataService,
        IThemeService themeService,
        ICache<UserData> userDataCache)
    {
        _navigator = navFactory.GetRequiredNavigator(nameof(MainWindowViewModel));
        _appDataService = appDataService;
        _themeService = themeService;
        _userDataCache = userDataCache;
        _navigator.Navigated += Navigator_Navigated;
        Content = _navigator.CurrentRoute.Content;
    }

    ~MainWindowViewModel() => _navigator.Navigated -= Navigator_Navigated;

    public async Task InitializeContentAsync()
    {
        try
        {
            IsInitializing = true;

            Result<SavedData<UserData>?> loadUserDataResult = await _appDataService.LoadUserDataAsync(updateUserCache: true);
            if (!loadUserDataResult.IsSuccessful)
                throw new Exception($"{nameof(MainWindowViewModel)} failed to load user data: {loadUserDataResult.Message}", loadUserDataResult.Exception);

            SavedData<UserData>? userData = loadUserDataResult.Value;
            AppThemeSettings? themeSettings = userData?.Data.Settings?.Theme;

            if (themeSettings != null)
            {
                _themeService.SetTheme(themeSettings.ThemeType);
                _themeService.SetBaseFontSize(themeSettings.FontSize);
            }

            if (userData == null)
                NavigateToSetup();
            else
                NavigateToLockscreen();
        }
        finally
        {
            IsInitializing = false;
        }
    }

    public void NavigateToSetup() => _navigator.Navigate(nameof(SetupViewModel));

    public void NavigateToLockscreen() => _navigator.Navigate(nameof(LockScreenViewModel));

    public async Task LoadSavedDataAsync()
    {
        UserData userData = _userDataCache.Get() ?? throw new InvalidOperationException($"{nameof(MainWindowViewModel)} failed to load user data - {nameof(UserData)} cache has no value.");

        Result<SavedData<EntityData>?> loadEntitiesResult = await _appDataService.LoadEntityDataAsync(forUserId: userData.UserId, updateRepositories: true);
        if (!loadEntitiesResult.IsSuccessful)
            throw new Exception($"{nameof(MainWindowViewModel)} failed to load saved entity data: {loadEntitiesResult.Message}", loadEntitiesResult.Exception);
    }

    public void NavigateToHome() => _navigator.Navigate(nameof(HomeViewModel));

    private void Navigator_Navigated(object? sender, CurrentRoute e) => Content = e.Content;
}