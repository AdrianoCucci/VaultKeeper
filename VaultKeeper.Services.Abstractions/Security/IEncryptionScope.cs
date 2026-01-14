namespace VaultKeeper.Services.Abstractions.Security;

public interface IEncryptionScope
{
    string Encrypt(string data);

    string Decrypt(string encryptedText);
}
