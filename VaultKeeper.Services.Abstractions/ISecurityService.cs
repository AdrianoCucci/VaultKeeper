using System.Text;
using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions;

public interface ISecurityService
{
    Result<string> Decrypt(string value);
    Result<string> Encrypt(string value);
    Result<string> CreateHash(string value, Encoding? encoding = null);
    Result<bool> CompareHash(string value, string hash, Encoding? encoding = null);
}