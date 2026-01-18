using Avalonia.Controls;

namespace VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;

public interface IViewLocatorService
{
    TView GetRequiredView<TViewModel, TView>(TViewModel? viewModel = null)
        where TViewModel : class
        where TView : Control;
    Control GetRequiredView<TViewModel>(TViewModel? viewModel = null) where TViewModel : class;
    TView? GetView<TViewModel, TView>(TViewModel? viewModel = null)
        where TViewModel : class
        where TView : Control;
    Control? GetView<TViewModel>(TViewModel? viewModel = null) where TViewModel : class;
}
