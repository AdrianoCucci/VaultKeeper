using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Constants;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class MainContentViewModel : ViewModelBase
{
    public INavigator Navigator { get; }

    public ObservableCollection<NavItemViewModel> TabNavItems { get; }

    [ObservableProperty]
    private NavItemViewModel? _selectedTab;

    [ObservableProperty]
    private object? _navigatorContent;

    public MainContentViewModel(INavigator navigator)
    {
        TabNavItems =
        [
            new(new()
            {
                Key = nameof(VaultPageViewModel),
                NavContent = CreateControl<StackPanel>(panel =>
                {
                    panel.Orientation = Avalonia.Layout.Orientation.Horizontal;
                    panel.Children.Add(CreateControl<PathIcon>(x =>
                    {
                        x.Margin = new(0, 0, 6, 0);
                        x.Bind(PathIcon.DataProperty, x.Resources.GetResourceObservable(Icons.Vault));
                    }));
                    panel.Children.Add(CreateControl<TextBlock>(x => x.Text = "Vault"));
                }),
            }),
            new(new()
            {
                Key = "SettingsPageViewModel",
                NavContent = CreateControl<StackPanel>(panel =>
                {
                    panel.Orientation = Avalonia.Layout.Orientation.Horizontal;
                    panel.Children.Add(CreateControl<PathIcon>(x =>
                    {
                        x.Margin = new(0, 0, 6, 0);
                        x.Bind(PathIcon.DataProperty, x.Resources.GetResourceObservable(Icons.Gear));
                    }));
                    panel.Children.Add(CreateControl<TextBlock>(x => x.Text = "Settings"));
                })
            }),
        ];

        Navigator = navigator;

        UpdateCurrentNavigation(Navigator.CurrentRoute);
        Navigator.Navigated += Navigator_Navigated;
    }

#if DEBUG
    public MainContentViewModel()
    {
        Navigator = null!;
        _selectedTab = null!;
        TabNavItems = [];
    }
#endif

    ~MainContentViewModel()
    {
        if (Navigator != null)
            Navigator.Navigated -= Navigator_Navigated;
    }

    public void UpdateSelectedTabState()
    {
        string? tabKey = SelectedTab?.Model.Key;
        if (tabKey != null)
            Navigator.Navigate(tabKey);
    }

    private void UpdateCurrentNavigation(CurrentRoute currentRoute) => NavigatorContent = currentRoute.Content?.Invoke();

    private void Navigator_Navigated(object? sender, CurrentRoute e) => UpdateCurrentNavigation(e);
}