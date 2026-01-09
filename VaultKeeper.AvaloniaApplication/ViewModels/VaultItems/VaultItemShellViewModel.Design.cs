namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;

public partial class VaultItemShellViewModel
{
    public static VaultItemShellViewModel Design { get; } = new(new VaultItemViewModel(new() { Name = "Name", Value = "Value" }));
}
