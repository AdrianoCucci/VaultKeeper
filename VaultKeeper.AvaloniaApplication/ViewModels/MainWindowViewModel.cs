using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public INavigator Navigator { get; }

    [ObservableProperty]
    private object? _navigatorContent;

    public MainWindowViewModel(INavigator navigator)
    {
        Navigator = navigator;

        //UpdateNavigationContent(Navigator.CurrentRoute);
        //Navigator.Navigated += Navigator_Navigated;
    }

    ~MainWindowViewModel() => Navigator.Navigated -= Navigator_Navigated;

    private void UpdateNavigationContent(CurrentRoute currentRoute) => NavigatorContent = currentRoute.Content?.Invoke();

    private void Navigator_Navigated(object? sender, CurrentRoute e) => UpdateNavigationContent(e);
}