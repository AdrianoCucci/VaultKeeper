using CommunityToolkit.Mvvm.ComponentModel;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupShellViewModel(GroupViewModelBase content) : ViewModelBase
{
    [ObservableProperty]
    private GroupViewModelBase _content = content;
}