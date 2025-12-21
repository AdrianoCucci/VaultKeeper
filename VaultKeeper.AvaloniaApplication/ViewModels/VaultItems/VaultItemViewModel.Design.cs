using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

[ExcludeFromCodeCoverage]
public partial class VaultItemViewModel
{
    public static readonly DesignContext Design = new();

    public class DesignContext() : VaultItemViewModel(new()
    {
        Name = "Name",
        Value = "Value"
    });
}
