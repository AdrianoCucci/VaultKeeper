using System;

namespace VaultKeeper.Models.ApplicationData;

public record UserData
{
    public Guid UserId { get; init; }
    public string? MainPasswordHash { get; init; }
    public string? CustomEntitiesDataPath { get; init; }
}
