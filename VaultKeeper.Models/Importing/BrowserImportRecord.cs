using VaultKeeper.Models.Attributes;

namespace VaultKeeper.Models.Importing;

// Chromium and Firefox browsers have the same column headers.
public record BrowserImportRecord
{
    [CsvColumn(Header = "url")]
    public string Url { get; set; } = string.Empty;

    [CsvColumn(Header = "username")]
    public string Username { get; set; } = string.Empty;

    [CsvColumn(Header = "password")]
    public string Password { get; set; } = string.Empty;
}