using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.Common.Models.Queries;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Groups;

public partial class GroupDeleteOptionsViewModel : ViewModelBase
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(DeleteWithChildren))]
    private CascadeDeleteMode _cascadeDeleteMode = CascadeDeleteMode.OrphanChildren;

    public bool DeleteWithChildren => CascadeDeleteMode == CascadeDeleteMode.DeleteChildren;
}