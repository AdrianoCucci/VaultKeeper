using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Services;
using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.Views;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Extensions.DependencyInjection;

namespace VaultKeeper.AvaloniaApplication;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted()
    {
        _serviceProvider = ConfigureServices();

        IAppDataService appDataService = _serviceProvider.GetRequiredService<IAppDataService>();
        _ = await appDataService.LoadUserDataAsync();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
            desktop.ShutdownRequested += ShutdownRequested;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        IAppDataService? appDataService = _serviceProvider?.GetRequiredService<IAppDataService>();
        appDataService?.SaveAllDataAsync().Wait();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .AddVaultKeeperServices()
            .AddSingleton<IPlatformService, PlatformService>()

            .AddScoped<MainWindowViewModel>()
            .AddScoped<VaultPageViewModel>();

        if (ApplicationLifetime != null)
            services.AddSingleton(ApplicationLifetime);

        return services.BuildServiceProvider();
    }
}