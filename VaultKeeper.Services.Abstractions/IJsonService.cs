using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions;

public interface IJsonService
{
    Result<string> Serialize<T>(T data);
    Result<T> Deserialize<T>(string json);
}