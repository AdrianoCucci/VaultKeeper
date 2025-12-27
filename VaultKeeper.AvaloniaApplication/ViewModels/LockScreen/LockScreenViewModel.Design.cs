namespace VaultKeeper.AvaloniaApplication.ViewModels.LockScreen;

public partial class LockScreenViewModel : ViewModelBase
{
    public static readonly DesignContext Design = new();

    public class DesignContext() : LockScreenViewModel(null!, null!);
}
