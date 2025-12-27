using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.Abstractions.Navigation;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? _content = null;

    private readonly INavigator _navigator;

    public MainWindowViewModel(INavigatorFactory navFactory)
    {
        _navigator = navFactory.GetRequiredNavigator(nameof(MainWindowViewModel));
        _navigator.Navigated += Navigator_Navigated;
        Content = _navigator.CurrentRoute.Content;
    }

    ~MainWindowViewModel() => _navigator.Navigated -= Navigator_Navigated;

    public void NavigateToHome() => _navigator.Navigate(nameof(HomeViewModel));

    private void Navigator_Navigated(object? sender, CurrentRoute e) => Content = e.Content;
}