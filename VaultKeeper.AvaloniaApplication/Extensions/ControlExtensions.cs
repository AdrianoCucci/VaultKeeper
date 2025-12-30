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
}
