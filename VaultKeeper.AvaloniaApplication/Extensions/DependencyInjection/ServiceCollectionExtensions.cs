using Microsoft.Extensions.DependencyInjection;
using System;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;
using VaultKeeper.AvaloniaApplication.Services;
using VaultKeeper.AvaloniaApplication.Services.ViewLocation;
using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.AvaloniaApplication.ViewModels.Importing;
using VaultKeeper.AvaloniaApplication.ViewModels.Settings;
using VaultKeeper.AvaloniaApplication.ViewModels.Setup;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultPage;
using VaultKeeper.AvaloniaApplication.Views;
using VaultKeeper.AvaloniaApplication.Views.Common.Prompts;
using VaultKeeper.AvaloniaApplication.Views.Groups;
using VaultKeeper.AvaloniaApplication.Views.Importing;
using VaultKeeper.AvaloniaApplication.Views.Settings;
using VaultKeeper.AvaloniaApplication.Views.Setup;
using VaultKeeper.AvaloniaApplication.Views.VaultPage;

namespace VaultKeeper.AvaloniaApplication.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAvaloniaServices(this IServiceCollection services) => services
        .AddSingleton<IApplicationService, ApplicationService>()
        .AddSingleton<IPlatformService, PlatformService>()
        .AddSingleton<IBackupService, BackupService>()
        .AddSingleton<IThemeService, ThemeService>();

    public static IViewModelControlMapper MapVaultItemViewModelControls(this IViewModelControlMapper mapper) => mapper
        .Map<MainWindowViewModel, MainWindow>()

        .Map<SetupPageViewModel, SetupPageView>()
        .Map<SetupPageStep1ViewModel, SetupPageStep1View>()
        .Map<SetupPageStep2ViewModel, SetupPageStep2View>()

        .Map<LockScreenPageViewModel, LockScreenPageView>()
        .Map<HomeViewModel, HomeView>()
        .Map<VaultPageViewModel, VaultPageView>()
        .Map<SettingsPageViewModel, SettingsPageView>()

        .Map<ChangePasswordFormViewModel, ChangePasswordFormView>()
        .Map<VaultItemImportViewModel, VaultItemImportView>()
        .Map<GroupDeleteOptionsViewModel, GroupDeleteOptionsView>()
        .Map<GroupSelectInputViewModel, GroupSelectInputView>()
        .Map<KeyGenerationSettingsViewModel, KeyGenerationSettingsView>()
        .Map<EncryptionKeyFileViewModel, EncryptionKeyFileView>()

        .Map<PromptViewModel, PromptView>()
        .Map<ConfirmPromptViewModel, ConfirmPromptView>();

    public static IServiceCollection AddViewLocator(this IServiceCollection services, Action<ViewLocatorConfigOptions> optionsAction)
    {
        ViewLocatorConfigOptions options = new(services);
        optionsAction.Invoke(options);

        services.AddSingleton<IViewLocatorService, ViewLocatorService>();

        return services;
    }
}
