using CommunityToolkit.Mvvm.ComponentModel;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

public partial class PromptViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _header;

    [ObservableProperty]
    private string? _message;
}
