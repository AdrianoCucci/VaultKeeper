using System;
using System.Text;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Security;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services.Abstractions;

public interface ISecurityService
{
    //Result<EncryptionConfig> GenerateEncryptionConfig();
    Result<EncryptedData> Encrypt(string data);
    Result<string> Decrypt(EncryptedData data);
    Result<string> Decrypt(string data);
    Result<string> CreateHash(string value, Encoding? encoding = null);
    Result<bool> CompareHash(string value, string hash, Encoding? encoding = null);
    Result UsingEncryptionScope(Action<IEncryptionScope> scopeFunc);
}