using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.ConfirmPrompts;

public partial class ConfirmPromptViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _header;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private object? _contextObject;

    public Action? CancelAction { get; set; }
    public Func<Task>? ConfirmAction { get; set; }

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
}
