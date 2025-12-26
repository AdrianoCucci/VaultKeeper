using Microsoft.Extensions.DependencyInjection;
using VaultKeeper.Models;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Extensions.DependencyInjection;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVaultKeeperServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<ISecurityService, SecurityService>()
            .AddSingleton<IJsonService, JsonService>()
            .AddSingleton<IFileService, FileService>()
            .AddSingleton<IAppDataService, AppDataService>()
            .AddScoped<IVaultItemService, VaultItemService>()

            .AddInMemoryRepository<VaultItem>()
            .AddInMemoryRepository<Group>();
    }
}
