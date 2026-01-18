using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;
using VaultKeeper.AvaloniaApplication.Extensions.DependencyInjection;
using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.ViewModels.Importing;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;
using VaultKeeper.AvaloniaApplication.ViewModels.Setup;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultPage;
using VaultKeeper.AvaloniaApplication.Views;
using VaultKeeper.Common.Logging;
using VaultKeeper.Common.Logging.Extensions.DependencyInjection;
using VaultKeeper.Models.Navigation;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Extensions.DependencyInjection;

namespace VaultKeeper.AvaloniaApplication;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private ILogger<App>? _logger;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted()
    {
        _serviceProvider = ConfigureServices();
        _logger = _serviceProvider.GetService<ILogger<App>>();

        _logger?.LogInformation("==================== APPLICATION STARTED ====================");

        IViewLocatorService viewLocatorService = _serviceProvider.GetRequiredService<IViewLocatorService>();

        DataTemplates.Add(new ViewLocator(viewLocatorService));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow { DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>() };
            desktop.ShutdownRequested += ShutdownRequested;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        _logger?.LogInformation("==================== APPLICATION SHUT DOWN ====================");

        IAppSessionService? appSessionService = _serviceProvider?.GetRequiredService<IAppSessionService>();
        if (appSessionService == null) return;

        appSessionService.LogoutAsync().Wait();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private ServiceProvider ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection()
            .ConfigureLogging(new()
            {
                LogLevel = Common.Logging.LogLevel.Information,
                FileLoggingConfig = new()
                {
                    FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VaultKeeper", "logs", "log.txt"),
                    RollingInterval = FileLoggingRollingInterval.Day,
                }
            })
            .AddVaultKeeperServices()
            .AddAvaloniaServices()
            .AddViewLocator(options =>
            {
                options.MapViewModelControls(mapper => mapper.MapVaultItemViewModelControls());
            });

        services.AddNavigation(sp => new HashSet<RouteScope>()
        {
            new()
            {
                Key = nameof(MainWindowViewModel),
                Routes =
                [
                    new() { Key = nameof(MainWindowViewModel) }, // Initializing route
                    new()
                    {
                        Key = nameof(SetupPageViewModel),
                        Content = sp.GetRequiredService<SetupPageViewModel>
                    },
                    new()
                    {
                        Key = nameof(LockScreenPageViewModel),
                        Content = sp.GetRequiredService<LockScreenPageViewModel>
                    },
                    new()
                    {
                        Key = nameof(HomeViewModel),
                        Content = sp.GetRequiredService<HomeViewModel>
                    }
                ]
            },
            new()
            {
                Key = nameof(HomeViewModel),
                Routes =
                [
                    new()
                    {
                        Key = nameof(VaultPageViewModel),
                        Content = sp.GetRequiredService<VaultPageViewModel>
                    },
                    new()
                    {
                        Key = nameof(SettingsPageViewModel),
                        Content = sp.GetRequiredService<SettingsPageViewModel>
                    }
                ]
            }
        });

        return services.BuildServiceProvider();
    }
}