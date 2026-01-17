using System;
using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions.Security;

public interface IEncryptionService
{
    Result<string> GenerateEncryptionKey();
    Result<string> GenerateEncryptionKeyFileData(string? key = null);
    Result<string> Encrypt(string data, bool forceUseSystemKey = false);
    Result<string> Decrypt(string data, bool forceUseSystemKey = false);
    Result UsingEncryptionScope(Action<IEncryptionScope> scopeFunc);
    Result VerifyValidEncryptionKeyFile(string? filePath);
    void UseSystemEncryptionKey();
    Result UseEncryptionKeyFile(string filePath);
    bool IsUsingDefaultEncryptionKey();
}