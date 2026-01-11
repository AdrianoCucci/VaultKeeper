using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Importing;

namespace VaultKeeper.Services.Abstractions.Importing;

public interface IImportService
{
    IEnumerable<ImportSource> GetImportSources();
    Task<Result> ProcessImportAsync(ImportSourceType sourceType, string sourceFilePath);
}