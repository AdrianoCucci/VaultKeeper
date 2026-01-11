using System.Collections.Generic;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.Models.Importing;

public record ExportData
{
    public required IEnumerable<VaultItem> VaultItems { get; set; }
    public required IEnumerable<Group> Groups { get; set; }
}
