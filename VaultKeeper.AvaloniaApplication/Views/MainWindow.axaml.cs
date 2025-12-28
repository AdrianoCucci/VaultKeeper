using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    protected override async void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            await viewModel.InitializeContentAsync();

        base.OnApplyTemplate(e);
    }

    private void LockScreenView_LoginSuccess(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.NavigateToHome();
    }
}