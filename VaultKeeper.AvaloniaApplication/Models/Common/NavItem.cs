namespace VaultKeeper.AvaloniaApplication.Models.Common;

public record NavItem
{
    public string? Key { get; set; }
    public object? NavContent { get; set; }
    public object? MainContent { get; set; }
}
