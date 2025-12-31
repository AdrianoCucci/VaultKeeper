using System;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupViewModel
{
    public static GroupViewModel Design => new(new()
    {
        Id = Guid.NewGuid(),
        Name = "Group Name"
    });
}
