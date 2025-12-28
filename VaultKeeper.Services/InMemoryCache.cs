using Microsoft.Extensions.Logging;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class InMemoryCache<T>(ILogger<InMemoryCache<T>> logger) : ICache<T>
{
    private T? _state;

    public T? Get()
    {
        logger.LogInformation(nameof(Get));
        return _state;
    }

    public void Set(T value)
    {
        logger.LogInformation(nameof(Set));
        _state = value;
    }

    public void Clear()
    {
        logger.LogInformation(nameof(Clear));
        _state = default;
    }
}
