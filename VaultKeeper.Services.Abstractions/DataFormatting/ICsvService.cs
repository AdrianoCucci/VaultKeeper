using System.Collections.Generic;
using VaultKeeper.Common.Results;

namespace VaultKeeper.Services.Abstractions.DataFormatting;

public interface ICsvService
{
    Result<string> Serialize<T>(IEnumerable<T> objs) where T : class;
    Result<IEnumerable<T>> Deserialize<T>(string csv) where T : class, new();
}