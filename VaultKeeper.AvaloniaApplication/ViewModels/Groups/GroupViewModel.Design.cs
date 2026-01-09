using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupViewModel
{
    [ObservableProperty]
    private bool _isReadOnly = false;

    public static GroupViewModel Design => new(new()
    {
        Id = Guid.NewGuid(),
        Name = "Group Name"
    });
}
