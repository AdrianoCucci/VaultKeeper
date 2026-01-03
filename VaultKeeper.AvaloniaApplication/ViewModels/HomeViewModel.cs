using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Constants;
using VaultKeeper.AvaloniaApplication.Extensions;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;
using VaultKeeper.Models.Navigation;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IAppSessionService _appSessionService;

    [ObservableProperty]
    private ObservableCollection<NavItemViewModel> _tabNavItems;

    [ObservableProperty]
    private NavItemViewModel? _selectedTab;

    [ObservableProperty]
    private object? _content;

    public HomeViewModel(INavigatorFactory navFactory, IAppSessionService appSessionService, IApplicationService applicationService)
    {
        Application application = applicationService.GetApplication();

        _tabNavItems =
        [
            new(new()
            {
                Key = nameof(VaultPageViewModel),
                Label = "Vault",
                Icon = application.GetResourceOrDefault<Geometry>(Icons.Vault)
            }),
            new(new()
            {
                Key = nameof(SettingsPageViewModel),
                Label = "Settings",
                Icon = application.GetResourceOrDefault<Geometry>(Icons.Gear)
            }),
        ];

        _navigator = navFactory.GetRequiredNavigator(nameof(HomeViewModel));
        _navigator.Navigated += Navigator_Navigated;
        Content = _navigator.CurrentRoute.Content;
        _appSessionService = appSessionService;
    }

#if DEBUG
    public HomeViewModel()
    {
        _navigator = null!;
        _appSessionService = null!;
        _selectedTab = null!;
        _tabNavItems = [];
    }
#endif

    ~HomeViewModel()
    {
        if (_navigator != null)
            _navigator.Navigated -= Navigator_Navigated;
    }

    public void Initialize() => SelectedTab = TabNavItems.FirstOrDefault();

    public void UpdateSelectedTabState()
    {
        string? tabKey = SelectedTab?.Model.Key;
        if (tabKey != null)
            _navigator.Navigate(tabKey);
    }

    public async Task LogoutAsync()
    {
        if (_appSessionService == null) return;

        var logoutResult = await _appSessionService.LogoutAsync((nameof(MainWindowViewModel), nameof(LockScreenPageViewModel)));

        if (!logoutResult.IsSuccessful)
        {
            // TODO: handle error;
        }
    }

    private void Navigator_Navigated(object? sender, CurrentRoute e) => Content = e.Content;
}