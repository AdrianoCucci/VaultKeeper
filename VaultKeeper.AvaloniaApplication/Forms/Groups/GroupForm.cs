using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.Forms.Groups;

public partial class GroupForm(Group group, FormMode formMode = FormMode.New) : Form<Group>(formMode)
{
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required")]
    private string? _name;

    public override Group GetModel() => group with
    {
        Name = Name ?? string.Empty
    };
}
