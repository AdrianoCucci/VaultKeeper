using Microsoft.Extensions.DependencyInjection;
using VaultKeeper.Models;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Extensions.DependencyInjection;
using VaultKeeper.Services.Abstractions;

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
        return services
            .AddSingleton<ISecurityService, SecurityService>()
            .AddSingleton<IJsonService, JsonService>()
            .AddSingleton<IFileService, FileService>()
            .AddSingleton<IAppDataService, AppDataService>()
            .AddScoped<IVaultItemService, VaultItemService>()
            .AddInMemoryCache<UserData>()

            .AddInMemoryRepository<VaultItem>()
            .AddInMemoryRepository<Group>();
    }
}
