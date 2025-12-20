using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public partial class VaultItemEditForm(VaultItem? vaultItem = null) : FormModel<VaultItem>
{
    [ObservableProperty, NotifyDataErrorInfo, Required(ErrorMessage = "Name is required.")]
    private string? name = vaultItem?.Name;

    [ObservableProperty]
    private string? value = vaultItem?.Value;

    public override VaultItem GetModel() => (vaultItem ?? new()) with
    {
        Name = Name ?? string.Empty,
        Value = Value ?? string.Empty
    };
}