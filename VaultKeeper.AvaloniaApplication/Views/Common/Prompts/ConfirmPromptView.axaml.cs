using Avalonia;
using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public partial class ConfirmPromptView : ConfirmPromptViewBase<ConfirmPromptViewModel>
{
    public static readonly StyledProperty<object?> InnerContentProperty = AvaloniaProperty.Register<ConfirmPromptView, object?>(nameof(InnerContent));

    public object? InnerContent { get => GetValue(InnerContentProperty); set => SetValue(InnerContentProperty, value); }

    public ConfirmPromptView() => InitializeComponent();

    private void CancelButton_Clicked(object? sender, RoutedEventArgs e) => RaiseEvent(ConfirmPromptAction.Cancelled);

    private void ConfirmButton_Clicked(object? sender, RoutedEventArgs e) => RaiseEvent(ConfirmPromptAction.Confirmed);

    private void OverlayPromptView_Acknowledged(object? sender, RoutedEventArgs e) => Model?.HideOverlayPrompt();
}