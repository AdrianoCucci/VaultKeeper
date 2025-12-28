using System;
using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.ApplicationData.Files;

namespace VaultKeeper.Services.Abstractions;

public interface IAppDataService
{
    AppFileDefinition GetFileDefinition(AppFileType fileType);
    Task<Result> SaveAllDataAsync();
    Task<Result<SavedData<UserData>?>> SaveUserDataAsync(UserData? userData = null);
    Task<Result<SavedData<EntityData>>> SaveEntityDataAsync(EntityData? entityData = null, UserData? relatedUserData = null);
    Task<Result<SavedData<UserData>?>> LoadUserDataAsync(bool updateUserCache = false);
    Task<Result<SavedData<EntityData>?>> LoadEntityDataAsync(string? filePath = null, Guid? forUserId = null, bool updateRepositories = false);
}