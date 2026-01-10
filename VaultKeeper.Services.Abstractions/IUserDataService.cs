using System.Threading.Tasks;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;

namespace VaultKeeper.Services.Abstractions;

public interface IUserDataService
{
    Result<UserData?> GetUserData();
    Task<Result<UserData>> GetOrCreateUserDataAsync();
    Task<Result> SetMainPasswordAsync(string password);
    Task<Result> ChangeMainPasswordAsync(string currentPassword, string newPassword);
}