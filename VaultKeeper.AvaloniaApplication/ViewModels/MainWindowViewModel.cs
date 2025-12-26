using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VaultKeeper.AvaloniaApplication.Constants;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<NavItemViewModel> TabNavItems { get; }

    [ObservableProperty]
    private NavItemViewModel _selectedTab;

    public MainWindowViewModel(VaultPageViewModel vaultPageViewModel)
    {
        TabNavItems =
        [
            new(new()
            {
                Key = "Vault",
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
                MainContent = vaultPageViewModel
            }),
            new(new()
            {
                Key = "Settings",
                NavContent = CreateControl<StackPanel>(panel =>
                {
                    panel.Orientation = Avalonia.Layout.Orientation.Horizontal;
                    panel.Children.Add(CreateControl<PathIcon>(x =>
                    {
                        x.Margin = new(0, 0, 6, 0);
                        x.Bind(PathIcon.DataProperty, x.Resources.GetResourceObservable(Icons.Gear));
                    }));
                    panel.Children.Add(CreateControl<TextBlock>(x => x.Text = "Settings"));
                }),
                MainContent = new VaultItemViewModel(new()
                {
                    Name = "My Account",
                    Value = "Password123"
                })
            }),
        ];

        _selectedTab = TabNavItems[0];
    }
}