using VaultKeeper.Models.Attributes;

namespace VaultKeeper.Models.Importing;

public record VaultItemImportRecord
{
    [CsvColumn]
    public string Name { get; set; } = string.Empty;

    [CsvColumn]
    public string Value { get; set; } = string.Empty;

    [CsvColumn(Header = "Group Name")]
    public string GroupName { get; set; } = string.Empty;
}