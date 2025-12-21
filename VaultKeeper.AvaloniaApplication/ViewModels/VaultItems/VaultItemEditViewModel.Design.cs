using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

[ExcludeFromCodeCoverage]
public partial class VaultItemEditViewModel
{
    public static readonly DesignContext Design = new();

    public class DesignContext() : VaultItemEditViewModel(new()
    {
        Name = "Name",
        Value = "Value"
    });
}
