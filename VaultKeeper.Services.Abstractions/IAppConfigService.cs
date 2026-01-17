using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;

namespace VaultKeeper.Services.Abstractions;

public interface IAppConfigService
{
    AppConfigData GetConfigDataOrDefault();
    Task<Result<AppConfigData>> LoadSavedAppConfigAsync();
    Task<Result<AppConfigData>> SetEncryptionKeyFilePathAsync(string? filePath);
}