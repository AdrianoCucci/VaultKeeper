using Microsoft.Extensions.DependencyInjection;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVaultKeeperServices(this IServiceCollection services)
    {
        return services
            .AddScoped<IVaultItemService, VaultItemService>();
    }
}
