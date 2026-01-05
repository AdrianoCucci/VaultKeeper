namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public partial class DeleteGroupConfirmPromptViewModel
{
    public new static DeleteGroupConfirmPromptViewModel Design { get; } = new()
    {
        Header = "Delete Group",
        Message = "What should happen to this groups keys upon deletion?"
    };
}
