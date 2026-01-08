using System;
using System.Threading.Tasks;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public partial class ConfirmPromptViewModel : PromptViewModel
{
    public Action? CancelAction { get; set; }
    public Func<ConfirmPromptViewModel, Task>? ConfirmAction { get; set; }

    private bool _isActionRunning = false;
    public bool IsActionRunning { get => _isActionRunning; private set => SetProperty(ref _isActionRunning, value); }

    public async Task InvokeActionAsync(ConfirmPromptAction action)
    {
        try
        {
            IsActionRunning = true;

            switch (action)
            {
                case ConfirmPromptAction.Cancelled:
                    CancelAction?.Invoke();
                    break;
                case ConfirmPromptAction.Confirmed:
                    if (ConfirmAction != null)
                        await ConfirmAction.Invoke(this);

                    break;
            }
        }
        finally
        {
            IsActionRunning = false;
        }
    }
}
