using Microsoft.Extensions.DependencyInjection;
using VaultKeeper.Models;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Extensions.DependencyInjection;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.DataFormatting;
using VaultKeeper.Services.DataFormatting;

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
            .AddSingleton<ISecurityService, SecurityService>()
            .AddSingleton<IJsonService, JsonService>()
            .AddSingleton<ICsvService, CsvService>()
            .AddSingleton<IFileService, FileService>()
            .AddSingleton<IAppDataService, AppDataService>()
            .AddScoped<IVaultItemService, VaultItemService>()

            .AddInMemoryCache<UserData>()
            .AddInMemoryRepository<VaultItem>()
            .AddInMemoryRepository<Group>();

        return services;
    }
}
