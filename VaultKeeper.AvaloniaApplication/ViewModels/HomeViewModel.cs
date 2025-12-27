using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VaultKeeper.AvaloniaApplication.Abstractions.Navigation;
using VaultKeeper.AvaloniaApplication.Constants;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
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

    public HomeViewModel(INavigatorFactory navFactory)
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