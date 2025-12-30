namespace VaultKeeper.AvaloniaApplication.ViewModels.Common;

public class EmptyViewModel : ViewModelBase
{
    public static EmptyViewModel Instance { get; } = new();

    private EmptyViewModel() { }
}
