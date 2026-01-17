using System;
using VaultKeeper.Models.Constants;
using VaultKeeper.Models.Importing;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Importing;

public partial class VaultItemImportViewModel
{
    private static readonly Lazy<VaultItemImportViewModel> _designLazy = new(() =>
    {
        ImportSource[] importSources =
        [
            new()
            {
                Type = ImportSourceType.Application,
                Name = "Vault Keeper",
                FileType = ".csv",
                Icon = Icons.VaultKeeper,
                Description = "A Vault Keeper keys CSV file."
            },
            new()
            {
                Type = ImportSourceType.Chromium,
                Name = "Chrome/Chromium",
                FileType = ".csv",
                Icon = Icons.GoogleChrome,
                Description = "A Google Chrome or other Chromium-based browser password CSV file.",
                AdditionalIcons = [Icons.MicrosoftEdge, Icons.BraveBrowser, Icons.OperaBrowser]
            },
            new()
            {
                Type = ImportSourceType.Firefox,
                Name = "Firefox",
                FileType = ".csv",
                Icon = Icons.MozillaFirefox,
                Description = "A Mozilla Firefox password CSV file."
            }
        ];

        return new()
        {
            Sources = [.. importSources],
            SelectedSource = importSources[0]
        };
    });

    public static VaultItemImportViewModel Design { get; } = _designLazy.Value;
}
