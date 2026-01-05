using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Errors;
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

    [ObservableProperty]
    private bool _isOverlayVisible = false;

    [ObservableProperty]
    private object? _overlayContent;

    private readonly IErrorReportingService _errorReportingService;
    private readonly INavigator _navigator;
    private readonly IAppDataService _appDataService;
    private readonly IThemeService _themeService;

    public MainWindowViewModel(
        IErrorReportingService errorReportingService,
        INavigatorFactory navFactory,
        IAppDataService appDataService,
        IThemeService themeService)
    {
        _errorReportingService = errorReportingService;
        _navigator = navFactory.GetRequiredNavigator(nameof(MainWindowViewModel));
        _errorReportingService = errorReportingService;
        _appDataService = appDataService;
        _themeService = themeService;

        _errorReportingService.ErrorReported += ErrorReportingService_ErrorReported;
        _navigator.Navigated += Navigator_Navigated;
    }

    ~MainWindowViewModel()
    {
        _errorReportingService.ErrorReported -= ErrorReportingService_ErrorReported;
        _navigator.Navigated -= Navigator_Navigated;
    }

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

    public void ShowOverlay(object content)
    {
        OverlayContent = content;
        IsOverlayVisible = true;
    }

    public void HideOverlay()
    {
        IsOverlayVisible = false;
        OverlayContent = null;
    }

    private void ErrorReportingService_ErrorReported(object? sender, Error error)
    {
        if (error.Visibility == ErrorVisibility.Internal) return;

        ShowOverlay(new PromptViewModel
        {
            Header = error.Header,
            Message = error.Message,
        });
    }

    public void NavigateToLockscreen() => _navigator.Navigate(nameof(LockScreenPageViewModel));

    public void NavigateToHome() => _navigator.Navigate(nameof(HomeViewModel));

    private void Navigator_Navigated(object? sender, CurrentRoute e) => Content = e.Content;
}