using VaultKeeper.Models.Security;

namespace VaultKeeper.Services.Abstractions.Security;

public interface IEncryptionScope
{
    EncryptedData Encrypt(string data);

    string Decrypt(EncryptedData encryptedData);

    string Decrypt(string encryptedText);
}
