using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.Models.Common;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common;

public partial class NavItemViewModel(NavItem navItem) : ViewModelBase<NavItem>(navItem)
{
    [ObservableProperty]
    private object? _navContent = navItem.NavContent;

    [ObservableProperty]
    private object? _mainContent = navItem.MainContent;
}
