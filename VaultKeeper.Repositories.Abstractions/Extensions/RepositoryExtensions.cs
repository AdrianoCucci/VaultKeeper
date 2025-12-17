namespace VaultKeeper.Repositories.Abstractions.Extensions;

public static class RepositoryExtensions
{
    public static IReadOnlyRepository<T> AsReadOnly<T>(this IRepository<T> repository) => repository;
}
