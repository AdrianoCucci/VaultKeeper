using System.Text;

namespace VaultKeeper.Services.Abstractions.Security;

public interface IEncryptor
{
    byte[] EncryptToBytes(byte[] data, Encoding? encoding = null);
    byte[] EncryptToBytes(string data, Encoding? encoding = null);
    string EncryptToString(byte[] data, Encoding? encoding = null);
    string EncryptToString(string data, Encoding? encoding = null);
}