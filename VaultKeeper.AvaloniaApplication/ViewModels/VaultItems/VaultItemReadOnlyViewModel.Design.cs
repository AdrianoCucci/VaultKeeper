using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

[ExcludeFromCodeCoverage]
public partial class VaultItemReadOnlyViewModel
{
    public static readonly DesignContext Design = new();

    public class DesignContext() : VaultItemReadOnlyViewModel(new()
    {
        Name = "Name",
        Value = "Value"
    });
}
