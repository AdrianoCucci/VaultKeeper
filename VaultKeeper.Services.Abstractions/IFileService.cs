using System.Text;
using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions;

public interface IFileService
{
    Result<byte[]> ReadFileBytes(string path);
    Result<string> ReadFileText(string path, Encoding? encoding = null);
    Result WriteFileBytes(string path, byte[] data);
    Result WriteFileText(string path, string data, Encoding? encoding = null);
}