using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.Services;

public class UserDataService(
    ILogger<UserDataService> logger,
    ICache<UserData> userDataCache,
    IHashService hashService,
    IAppDataService appDataService) : IUserDataService
{
    public Result<UserData?> GetUserData()
    {
        logger.LogInformation(nameof(GetUserData));
        return userDataCache.Get().ToOkResult().Logged(logger);
    }

    public async Task<Result<UserData>> GetOrCreateUserDataAsync()
    {
        logger.LogInformation(nameof(GetOrCreateUserDataAsync));

        UserData? userData = userDataCache.Get();
        if (userData != null)
            return userData.ToOkResult().Logged(logger);

        logger.LogInformation("User data not found in cache - creating & setting new instance.");

        userData = new() { UserId = Guid.NewGuid() };

        Result<SavedData<UserData>?> saveDataResult = await appDataService.SaveUserDataAsync(userData);
        if (saveDataResult.IsSuccessful)
            return saveDataResult.WithValue<UserData>().Logged(logger);

        userDataCache.Set(saveDataResult.Value!.Data with { });

        return userData.ToOkResult().Logged(logger);
    }

    public async Task<Result> SetMainPasswordAsync(string password)
    {
        logger.LogInformation(nameof(SetMainPasswordAsync));

        UserData userData = userDataCache.Get() ?? new() { UserId = Guid.NewGuid() };

        Result<string> createHashResult = hashService.CreateHash(password);
        if (!createHashResult.IsSuccessful)
            return createHashResult.Logged(logger);

        Result saveDataResult = await SaveUserDataAsync(userData with
        {
            MainPasswordHash = createHashResult.Value!
        });

        return saveDataResult.Logged(logger);
    }

    public async Task<Result> ChangeMainPasswordAsync(string currentPassword, string newPassword)
    {
        logger.LogInformation(nameof(ChangeMainPasswordAsync));

        UserData? userData = userDataCache.Get();
        string? currentPasswordHash = userData?.MainPasswordHash;

        if (string.IsNullOrWhiteSpace(currentPasswordHash))
            return Result.Failed(ResultFailureType.NotFound, "User password hash not set in User Cache.");

        var compareCurrentPasswordResult = hashService.CompareHash(currentPassword, currentPasswordHash);
        if (!compareCurrentPasswordResult.IsSuccessful)
            return compareCurrentPasswordResult.Logged(logger);

        bool currentPasswordIsValid = compareCurrentPasswordResult.Value;
        if (!currentPasswordIsValid)
            return Result.Failed(ResultFailureType.BadRequest, "Current password is invalid.");

        Result<string> newPasswordHashResult = hashService.CreateHash(newPassword);
        if (!newPasswordHashResult.IsSuccessful)
            return newPasswordHashResult.Logged(logger);

        Result saveDataResult = await SaveUserDataAsync(userData! with
        {
            MainPasswordHash = newPasswordHashResult.Value!
        });

        return saveDataResult.Logged(logger);
    }

    private async Task<Result> SaveUserDataAsync(UserData userData)
    {
        logger.LogInformation(nameof(SaveUserDataAsync));

        Result<SavedData<UserData>?> saveDataResult = await appDataService.SaveUserDataAsync(userData);
        if (!saveDataResult.IsSuccessful)
            return saveDataResult.WithValue<UserData>().Logged(logger);

        userDataCache.Set(saveDataResult.Value!.Data with { });

        return Result.Ok("User data saved successfully.").Logged(logger);
    }
}
