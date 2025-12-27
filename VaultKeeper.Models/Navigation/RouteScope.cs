namespace VaultKeeper.Models.Navigation;

public record RouteScope
{
    public required string Key { get; init; }
    public required Route[] Routes { get; init; }
    public string? DefaultRouteKey { get; init; }
}
