using VaultKeeper.Common.Results;

namespace VaultKeeper.Services;

public interface ICache<T>
{
    Result<T?> Get();

    Result Set(T value);

    Result Clear();
}
