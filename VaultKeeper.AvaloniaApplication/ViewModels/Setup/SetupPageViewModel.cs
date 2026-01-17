using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using VaultKeeper.Models.Navigation.Extensions;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Setup;

public partial class SetupPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? _content;

    [ObservableProperty]
    private bool _canNavigateBack = false;

    private readonly IServiceProvider _serviceProvider;

    public SetupPageViewModel(IServiceProvider serviceProvider, INavigatorFactory? navFactory = null)
    {
        _serviceProvider = serviceProvider;

        INavigator? navigator = navFactory?.GetNavigator(nameof(MainWindowViewModel));
        if (navigator != null)
            _canNavigateBack = navigator.CurrentRoute.GetParamOrDefault<bool>(nameof(CanNavigateBack));

        GoToStep1();
    }

#if DEBUG
    public SetupPageViewModel()
    {
        _serviceProvider = null!;
        _content = new SetupPageStep1ViewModel();
    }
#endif

    public void GoToStep1() => Content = _serviceProvider.GetRequiredService<SetupPageStep1ViewModel>();

    public void GoToStep2()
    {
        Content = _serviceProvider.GetRequiredService<SetupPageStep2ViewModel>();
        CanNavigateBack = false;
    }
}
