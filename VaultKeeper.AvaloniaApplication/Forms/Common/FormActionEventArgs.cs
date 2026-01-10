using Avalonia.Interactivity;

namespace VaultKeeper.AvaloniaApplication.Forms.Common;

public class FormActionEventArgs<TForm>(RoutedEvent routedEvent, object? source = null) : RoutedEventArgs(routedEvent, source) where TForm : Form
{
    public required TForm Form { get; init; }
    public required FormAction Action { get; init; }
}
