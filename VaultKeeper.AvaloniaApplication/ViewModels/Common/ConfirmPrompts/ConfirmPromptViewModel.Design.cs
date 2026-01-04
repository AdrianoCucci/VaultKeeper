namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.ConfirmPrompts;

public partial class ConfirmPromptViewModel
{
    public static ConfirmPromptViewModel Design { get; } = new()
    {
        Header = "Delete Item",
        Message = "Are you sure you want to delete this item?"
    };
}
