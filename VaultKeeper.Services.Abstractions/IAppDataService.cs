using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;

namespace VaultKeeper.Services.Abstractions;

public interface IAppDataService
{
    Task<Result> SaveDataAsync();
    Task<Result<UserData>> LoadUserDataAsync();
}