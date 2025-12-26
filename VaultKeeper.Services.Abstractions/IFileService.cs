using System.IO;
using System.Text;
using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions;

public interface IFileService
{
    Result<byte[]> ReadFileBytes(string path);
    Result<string> ReadFileText(string path, Encoding? encoding = null);
    Result WriteFileBytes(string path, byte[] data, FileAttributes? attributes = null);
    Result WriteFileText(string path, string data, FileAttributes? attributes = null, Encoding? encoding = null);
}