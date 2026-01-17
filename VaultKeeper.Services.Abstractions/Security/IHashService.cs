using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions.Security;

public interface IHashService
{
    Result<string> CreateHash(string value);
    Result<bool> CompareHash(string value, string hash);
}