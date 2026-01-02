using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Navigation;

namespace VaultKeeper.Services;

public class AppSessionService(
    ILogger<AppSessionService> logger,
    ICache<UserData> userDataCache,
    IAppDataService appDataService,
    ISecurityService securityService,
    INavigatorFactory? navFactory = null) : IAppSessionService
{
    public async Task<Result> LoginAsync(UserData userData)
    {
        logger.LogInformation(nameof(LoginAsync));

        try
        {
            userDataCache.Set(userData);
            Result<SavedData<EntityData>?> loadEntitiesResult = await appDataService.LoadEntityDataAsync(forUserId: userData.UserId, updateRepositories: true);

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

        try
        {
            userDataCache.Clear();

            Result clearDataResult = await appDataService.ClearEntityDataAsync();
            if (!clearDataResult.IsSuccessful)
                return clearDataResult.Logged(logger);

            if (navFactory != null)
            {
                IEnumerable<INavigator> navigators = navFactory.GetAllNavigators();
                foreach (INavigator navigator in navigators)
                {
                    navigator.NavigateToDefaultRoute();
                }

                if (navRedirect.HasValue)
                {
                    (string scope, string route) = navRedirect.Value;

                    INavigator? navigator = navFactory.GetNavigator(scope);
                    navigator?.Navigate(route);
                }
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
