using System.Diagnostics.CodeAnalysis;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

[ExcludeFromCodeCoverage]
public partial class MainWindowViewModel
{
    public static readonly DesignContext Design = new();

    public class DesignContext() : MainWindowViewModel(new VaultPageViewModel.DesignContext());
}
