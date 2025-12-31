using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.Navigation;
using VaultKeeper.AvaloniaApplication.Constants;
using VaultKeeper.AvaloniaApplication.Extensions;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly INavigator _navigator;

    public ObservableCollection<NavItemViewModel> TabNavItems { get; }

    [ObservableProperty]
    private NavItemViewModel? _selectedTab;

    [ObservableProperty]
    private object? _content;

    public HomeViewModel(INavigatorFactory navFactory, IApplicationService applicationService)
    {
        Application application = applicationService.GetApplication();

        TabNavItems =
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
    }

#if DEBUG
    public HomeViewModel()
    {
        _navigator = null!;
        _selectedTab = null!;
        TabNavItems = [];
    }
#endif

    ~HomeViewModel()
    {
        if (_navigator != null)
            _navigator.Navigated -= Navigator_Navigated;
    }

    public void UpdateSelectedTabState()
    {
        string? tabKey = SelectedTab?.Model.Key;
        if (tabKey != null)
            _navigator.Navigate(tabKey);
    }

    private void Navigator_Navigated(object? sender, CurrentRoute e) => Content = e.Content;
}