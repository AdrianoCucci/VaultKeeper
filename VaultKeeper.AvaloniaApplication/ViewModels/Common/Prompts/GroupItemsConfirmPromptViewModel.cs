using VaultKeeper.AvaloniaApplication.ViewModels.Groups;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public partial class GroupItemsConfirmPromptViewModel(GroupSelectInputViewModel groupInputVM) : ConfirmPromptViewModel
{
    public GroupSelectInputViewModel GroupInputVM { get; init; } = groupInputVM;

    public GroupItemsConfirmPromptViewModel() : this(new()) { }
}
