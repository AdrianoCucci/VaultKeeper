using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;

namespace VaultKeeper.Services.Abstractions;

public interface IAppDataService
{
    Task<Result> SaveAllDataAsync();
    Task<Result<SavedData<UserData>?>> SaveUserDataAsync(UserData? userData = null);
    Task<Result<SavedData<EntityData>>> SaveEntityDataAsync(EntityData? entityData = null, string? filePath = null);
    Task<Result<SavedData<UserData>?>> LoadUserDataAsync();
    Task<Result<SavedData<EntityData>?>> LoadEntityDataAsync(string? filePath = null);
}