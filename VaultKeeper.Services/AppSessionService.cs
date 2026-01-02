using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Settings;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.Services;

public class AppSessionService(
    ILogger<AppSessionService> logger,
    ICache<UserData> userDataCache,
    ICache<UserSettings> userSettingsCache,
    IAppDataService appDataService,
    ISecurityService securityService,
    INavigatorFactory? navFactory = null) : IAppSessionService
{
    private bool _isLoggedIn = false;

    public async Task<Result> LoginAsync(UserData userData)
    {
        logger.LogInformation(nameof(LoginAsync));

        try
        {
            userDataCache.Set(userData);
            Result<SavedData<EntityData>?> loadEntitiesResult = await appDataService.LoadEntityDataAsync(forUserId: userData.UserId, updateRepositories: true);

            if (loadEntitiesResult.IsSuccessful)
                _isLoggedIn = true;

            return loadEntitiesResult.WithoutValue().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    public async Task<Result<bool>> TryLoginAsync(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result.Ok(false, "Password is invalid.").Logged(logger);

            Result<UserData?> getUserDataResult = await GetUserDataAsync();
            if (!getUserDataResult.IsSuccessful)
                return getUserDataResult.WithValue(false).Logged(logger);

            UserData userData = getUserDataResult.Value!;
            string? expectedPasswordHash = userData.MainPasswordHash;

            if (string.IsNullOrWhiteSpace(expectedPasswordHash))
                return Result.Failed<bool>(ResultFailureType.Conflict, "User has not setup a main password.").Logged(logger);

            Result<bool> compareHashResult = securityService.CompareHash(password, expectedPasswordHash);
            if (!compareHashResult.IsSuccessful)
                return compareHashResult.WithValue(false).Logged(logger);

            bool passwordIsMatch = compareHashResult.Value;
            if (!passwordIsMatch)
                return Result.Ok(false, "Password is invalid.").Logged(logger);

            Result loginResult = await LoginAsync(userData);

            return loginResult.WithValue(loginResult.IsSuccessful).Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult<bool>().Logged(logger);
        }
    }

    public async Task<Result> LogoutAsync((string NavRedirectScope, string NavRedirectRoute)? navRedirect = null)
    {
        logger.LogInformation(nameof(LogoutAsync));

        if (!_isLoggedIn)
        {
            logger.LogInformation("User is already not logged in - skipping logout process.");
            return Result.Ok().Logged(logger);
        }

        try
        {
            Result saveDataResult = await appDataService.SaveAllDataAsync();
            if (!saveDataResult.IsSuccessful)
                return saveDataResult.Logged(logger);

            BackupSettings? backupSettings = userSettingsCache.Get()?.Backup;
            if (backupSettings?.AutoBackupOnLogout == true)
            {
                var backupResult = await appDataService.SaveBackupAsync(backupSettings);
                if (!backupResult.IsSuccessful)
                    return backupResult.Logged(logger);
            }

            userDataCache.Clear();
            userSettingsCache.Clear();

            Result clearDataResult = await appDataService.ClearCachedEntityDataAsync();
            if (!clearDataResult.IsSuccessful)
                return clearDataResult.Logged(logger);

            if (navRedirect.HasValue && navFactory != null)
            {
                (string scope, string route) = navRedirect.Value;

                INavigator? navigator = navFactory.GetNavigator(scope);
                navigator?.Navigate(route);
            }

            return Result.Ok().Logged(logger);
        }
        catch (Exception ex)
        {
            return ex.ToFailedResult().Logged(logger);
        }
    }

    private async Task<Result<UserData?>> GetUserDataAsync()
    {
        logger.LogInformation(nameof(GetUserDataAsync));

        UserData? userData = userDataCache.Get();
        if (userData != null)
            return Result.Ok<UserData?>(userData);

        Result<SavedData<UserData>?> loadUserDataResult = await appDataService.LoadUserDataAsync(true);
        if (!loadUserDataResult.IsSuccessful)
            return loadUserDataResult.WithValue<UserData?>();

        userData = loadUserDataResult.Value?.Data;
        if (userData == null)
            return Result.Failed<UserData?>("Unable to fetch user data to compare login credentials");

        return Result.Ok<UserData?>(userData);
    }
}
