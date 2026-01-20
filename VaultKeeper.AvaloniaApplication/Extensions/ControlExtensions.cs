using Avalonia;
using Avalonia.Controls;

namespace VaultKeeper.AvaloniaApplication.Extensions;

public static class ControlExtensions
{
    public static void FocusEnd(this TextBox? textBox)
    {
        if (textBox == null) return;

        textBox.Focus();
        textBox.CaretIndex = textBox.Text?.Length ?? 0;
    }

    public static void ScrollUp(this ScrollViewer scrollViewer, double step)
    {
        Vector offset = scrollViewer.Offset;
        scrollViewer.Offset = offset.WithY(offset.Y - step);
    }

    public static void ScrollUp(this ScrollViewer scrollViewer) => ScrollUp(scrollViewer, 50);

    public static void ScrollDown(this ScrollViewer scrollViewer, double step)
    {
        Vector offset = scrollViewer.Offset;
        scrollViewer.Offset = offset.WithY(offset.Y + step);
    }

    public static void ScrollDown(this ScrollViewer scrollViewer) => ScrollDown(scrollViewer, 50);

    public static void ScrollToTop(this ScrollViewer scrollViewer) => scrollViewer.Offset = scrollViewer.Offset.WithY(0);
}
