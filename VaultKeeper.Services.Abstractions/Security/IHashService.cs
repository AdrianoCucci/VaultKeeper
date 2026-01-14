using System.Text;
using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions.Security;

public interface IHashService
{
    Result<string> CreateHash(string value, Encoding? encoding = null);
    Result<bool> CompareHash(string value, string hash, Encoding? encoding = null);
}