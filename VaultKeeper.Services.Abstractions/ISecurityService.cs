using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions;

public interface ISecurityService
{
    Result<string> Decrypt(string value);
    Result<string> Encrypt(string value);
}