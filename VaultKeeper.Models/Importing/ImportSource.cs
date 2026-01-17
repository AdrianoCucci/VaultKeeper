using System.Collections.Generic;

namespace VaultKeeper.Models.Importing;

public record ImportSource
{
    public required ImportSourceType Type { get; set; }
    public required string Name { get; set; }
    public required string FileType { get; set; }
    public string? Icon { get; set; }
    public IEnumerable<string> AdditionalIcons { get; set; } = [];
    public string? Description { get; set; }
}