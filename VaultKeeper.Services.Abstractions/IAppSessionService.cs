using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;

namespace VaultKeeper.Services.Abstractions;

public interface IAppSessionService
{
    Task<Result> LoginAsync(UserData userData);
    Task<Result<bool>> TryLoginAsync(string password);
    Task<Result> LogoutAsync((string NavRedirectScope, string NavRedirectRoute)? navRedirect = null);
}