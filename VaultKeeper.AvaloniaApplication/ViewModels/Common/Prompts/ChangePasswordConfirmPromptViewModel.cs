namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public partial class ChangePasswordConfirmPromptViewModel : ConfirmPromptViewModel
{
    public ChangePasswordFormViewModel FormVM { get; } = new();
}
