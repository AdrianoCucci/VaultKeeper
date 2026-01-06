using Avalonia;
using Avalonia.Interactivity;
using VaultKeeper.AvaloniaApplication.ViewModels.Common.Prompts;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public partial class PromptView : PromptViewBase<PromptViewModel>
{
    public static readonly StyledProperty<object?> InnerContentProperty = AvaloniaProperty.Register<PromptView, object?>(nameof(InnerContent));

    public static readonly StyledProperty<bool> ShowOkButtonProperty = AvaloniaProperty.Register<PromptView, bool>(nameof(ShowOkButton), true);

    public object? InnerContent { get => GetValue(InnerContentProperty); set => SetValue(InnerContentProperty, value); }
    public bool ShowOkButton { get => GetValue(ShowOkButtonProperty); set => SetValue(ShowOkButtonProperty, value); }

    public PromptView() => InitializeComponent();

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new(CloseButtonClickedEvent, this));
        RaiseEvent(new(AcknowledgedEvent, this));
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(AcknowledgedEvent, this));
}