using Microsoft.Extensions.DependencyInjection;
using VaultKeeper.Repositories.Abstractions;

namespace VaultKeeper.Repositories.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryRepository<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Add(new(typeof(IRepository<T>), typeof(InMemoryRepository<T>), lifetime));
        return services;
    }
}
