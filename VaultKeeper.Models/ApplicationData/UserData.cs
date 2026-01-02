using System;
using VaultKeeper.Models.Settings;

namespace VaultKeeper.Models.ApplicationData;

public record UserData
{
    public Guid UserId { get; init; } = Guid.Empty;
    public string? MainPasswordHash { get; init; }
    public UserSettings? Settings { get; init; }
}
