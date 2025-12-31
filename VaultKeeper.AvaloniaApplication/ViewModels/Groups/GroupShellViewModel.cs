using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.Models.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupShellViewModel(GroupViewModelBase content) : ViewModelBase
{
    [ObservableProperty]
    private GroupViewModelBase _content = content;

    public Group Model { get => Content.Model; set => Content.Model = value; }
}