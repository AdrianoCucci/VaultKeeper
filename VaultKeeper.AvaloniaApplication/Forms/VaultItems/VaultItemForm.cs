using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.Groups;
using VaultKeeper.Models.VaultItems;

namespace VaultKeeper.AvaloniaApplication.ViewModels.VaultItems.Common;

public partial class VaultItemForm(
    VaultItem vaultItem,
    FormMode mode = FormMode.New,
    GroupSelectInputViewModel? groupInputVM = null) : Form<VaultItem>(mode)
{
    public GroupSelectInputViewModel GroupInputVM { get; } = groupInputVM ?? new();

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required.")]
    private string? _name = vaultItem.Name;

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Value is required.")]
    private string? _value = vaultItem.Value;

    public VaultItem OriginalModel => vaultItem;

    public override VaultItem GetModel() => vaultItem with
    {
        Name = Name ?? string.Empty,
        Value = Value ?? string.Empty,
        GroupId = GroupInputVM.SelectedGroup?.Id
    };
}