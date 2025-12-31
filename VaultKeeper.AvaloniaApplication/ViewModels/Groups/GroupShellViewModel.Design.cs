using System;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupShellViewModel : ViewModelBase
{
    public static GroupShellViewModel Design { get; } = new(new GroupViewModel(new()
    {
        Id = Guid.Empty,
        Name = "Group Name"
    }));
}