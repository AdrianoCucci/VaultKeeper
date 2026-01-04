using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.Common.Models.Queries;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.ConfirmPrompts;

public partial class DeleteGroupConfirmPromptViewModel : ConfirmPromptViewModel
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(DeleteWithChildren))]
    private CascadeDeleteMode _cascadeDeleteMode;

    public bool DeleteWithChildren => CascadeDeleteMode == CascadeDeleteMode.DeleteChildren;
}
