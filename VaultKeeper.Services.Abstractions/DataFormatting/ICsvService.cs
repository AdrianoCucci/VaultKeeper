using System.Collections.Generic;
using System.Text;
using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions.DataFormatting;

public interface ICsvService
{
    Result<string> Serialize<T>(IEnumerable<T> objs, Encoding? encoding = null) where T : class;
    Result<IEnumerable<T>> Deserialize<T>(string csvText, Encoding? encoding = null) where T : class, new();
}