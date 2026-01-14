using System;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Security;

namespace VaultKeeper.Services.Abstractions.Security;

public interface IEncryptionService
{
    Result<string> Encrypt(string data);
    Result<string> Decrypt(string data);
    Result UsingEncryptionScope(Action<IEncryptionScope> scopeFunc);
    void UseEncryptionConfigFromFile(string? filePath);
    Result SaveEncryptionConfigToFile(string filePath, EncryptionConfig config);
}