using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions.Navigation;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Navigation;
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
    private readonly ICache<UserData> _userDataCache;

    public MainWindowViewModel(INavigatorFactory navFactory, IAppDataService appDataService, ICache<UserData> userDataCache)
    {
        _navigator = navFactory.GetRequiredNavigator(nameof(MainWindowViewModel));
        _appDataService = appDataService;
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

            Result<SavedData<UserData>?> loadUserDataResult = await _appDataService.LoadUserDataAsync();
            if (!loadUserDataResult.IsSuccessful)
                throw new Exception($"{nameof(MainWindowViewModel)} failed to load user data: {loadUserDataResult.Message}", loadUserDataResult.Exception);

            SavedData<UserData>? userData = loadUserDataResult.Value;
            if (userData == null)
            {
                NavigateToSetup();
                return;
            }

            Result cacheUserDataResult = _userDataCache.Set(userData.Data);
            if (!cacheUserDataResult.IsSuccessful)
                throw new Exception($"{nameof(MainWindowViewModel)} failed to cache loaded user data: {cacheUserDataResult.Message}", cacheUserDataResult.Exception);

            NavigateToLockscreen();
        }
        finally
        {
            IsInitializing = false;
        }
    }

    public void NavigateToSetup() => _navigator.Navigate(nameof(SetupViewModel));

    public void NavigateToLockscreen() => _navigator.Navigate(nameof(LockScreenViewModel));

    public void NavigateToHome() => _navigator.Navigate(nameof(HomeViewModel));

    private void Navigator_Navigated(object? sender, CurrentRoute e) => Content = e.Content;
}