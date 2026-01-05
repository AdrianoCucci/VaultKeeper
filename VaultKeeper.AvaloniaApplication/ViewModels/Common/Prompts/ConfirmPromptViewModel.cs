using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public partial class ConfirmPromptViewModel : PromptViewModel
{
    public Action? CancelAction { get; set; }
    public Func<Task>? ConfirmAction { get; set; }

    [ObservableProperty]
    private bool? _isOverlayVisible;

    [ObservableProperty]
    private PromptViewModel? _overlayPrompt;

    public async Task InvokeActionAsync(ConfirmPromptAction action)
    {
        switch (action)
        {
            case ConfirmPromptAction.Cancelled:
                CancelAction?.Invoke();
                break;
            case ConfirmPromptAction.Confirmed:
                if (ConfirmAction != null)
                    await ConfirmAction.Invoke();

                break;
        }
    }

    public void ShowOverlayPrompt(PromptViewModel prompt)
    {
        OverlayPrompt = prompt;
        IsOverlayVisible = true;
    }

    public void HideOverlayPrompt()
    {
        IsOverlayVisible = false;
        OverlayPrompt = null;
    }
}
