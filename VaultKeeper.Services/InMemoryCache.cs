using Microsoft.Extensions.Logging;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class InMemoryCache<T>(ILogger<InMemoryCache<T>> logger) : ICache<T>
{
    private T? _state;

    public Result<T?> Get() => _state.ToOkResult().Logged(logger);

    public Result Set(T value)
    {
        _state = value;
        return Result.Ok().Logged(logger);
    }

    public Result Clear()
    {
        _state = default;
        return Result.Ok().Logged(logger);
    }
}
