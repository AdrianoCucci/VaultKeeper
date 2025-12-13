namespace VaultKeeper.AvaloniaApplication.Models;

/// <summary>
/// This is our Model for a simple TodoItem.
/// </summary>
public record TodoItem
{
    /// <summary>
    /// Gets or sets the checked status of each item
    /// </summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Gets or sets the content of the to-do item
    /// </summary>
    public string? Content { get; set; }
}