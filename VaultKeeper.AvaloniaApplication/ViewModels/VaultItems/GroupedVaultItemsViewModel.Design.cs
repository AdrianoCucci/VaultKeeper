using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

[ExcludeFromCodeCoverage]
public partial class VaultItemViewModel
{
    public static readonly VaultItemViewModel Design = new(new()
    {
        Name = "Name",
        Value = "Value"
    });
}
