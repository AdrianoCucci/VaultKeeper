using VaultKeeper.AvaloniaApplication.Forms.Common;
using VaultKeeper.AvaloniaApplication.Forms.Groups;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public class GroupFormViewModel(Group group, FormMode formMode = FormMode.New) : GroupViewModelBase(group)
{
    public GroupForm Form { get; } = new(group, formMode);
}