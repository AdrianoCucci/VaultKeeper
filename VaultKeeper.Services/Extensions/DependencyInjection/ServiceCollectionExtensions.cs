using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.Navigation;
using VaultKeeper.Models.Settings;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Extensions.DependencyInjection;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.DataFormatting;
using VaultKeeper.Services.Abstractions.Groups;
using VaultKeeper.Services.Abstractions.Importing;
using VaultKeeper.Services.Abstractions.Navigation;
using VaultKeeper.Services.Abstractions.Security;
using VaultKeeper.Services.Abstractions.VaultItems;
using VaultKeeper.Services.DataFormatting;
using VaultKeeper.Services.Groups;
using VaultKeeper.Services.Importing;
using VaultKeeper.Services.Navigation;
using VaultKeeper.Services.Security;
using VaultKeeper.Services.VaultItems;

namespace VaultKeeper.Services.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryCache<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Add(new(typeof(ICache<T>), typeof(InMemoryCache<T>), lifetime));
        return services;
    }

    public static IServiceCollection AddVaultKeeperServices(this IServiceCollection services)
    {
        services
            .AddLogging()
            .AddSingleton<IErrorReportingService, ErrorReportingService>()
            .AddSingleton<IEncryptionService, EncryptionService>()
            .AddSingleton<IHashService, HashService>()
            .AddSingleton<IJsonService, JsonService>()
            .AddSingleton<ICsvService, CsvService>()
            .AddSingleton<IFileService, FileService>()
            .AddSingleton<IAppDataService, AppDataService>()
            .AddSingleton<IAppSessionService, AppSessionService>()
            .AddSingleton<IUserDataService, UserDataService>()
            .AddSingleton<IUserSettingsService, UserSettingsService>()
            .AddSingleton<ICharSetService, CharSetService>()
            .AddSingleton<IKeyGeneratorService, KeyGeneratorService>()
            .AddSingleton<IImportService, ImportService>()

            .AddScoped<IVaultItemService, VaultItemService>()
            .AddScoped<IVaultItemValidatorService, VaultItemValidatorService>()
            .AddScoped<IGroupService, GroupService>()
            .AddScoped<IGroupValidatorService, GroupValidatorService>()

            .AddInMemoryCache<UserData>()
            .AddInMemoryCache<UserSettings>()
            .AddInMemoryRepository<VaultItem>()
            .AddInMemoryRepository<Group>();

        return services;
    }

    public static IServiceCollection AddNavigation(this IServiceCollection services, Func<IServiceProvider, IEnumerable<RouteScope>> routesFunc)
    {
        return services.AddSingleton<INavigatorFactory>(sp => new NavigatorFactory(sp, routesFunc.Invoke(sp)));
    }
}
