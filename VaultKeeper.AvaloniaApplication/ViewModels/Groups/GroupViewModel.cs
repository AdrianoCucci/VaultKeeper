using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupViewModel(Group group) : GroupViewModelBase(group)
{
    [ObservableProperty]
    private bool _isReadOnly = false;
}