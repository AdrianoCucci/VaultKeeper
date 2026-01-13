using System.Text;

namespace VaultKeeper.Services.Abstractions.Security;

public interface IDecryptor
{
    byte[] DecryptToBytes(string data, Encoding? encoding = null);
    byte[] DecryptToBytes(byte[] data, Encoding? encoding = null);
    string DecryptToString(byte[] data, Encoding? encoding = null);
    string DecryptToString(string data, Encoding? encoding = null);
}