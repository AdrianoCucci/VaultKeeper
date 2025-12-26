using Avalonia;
using Avalonia.Controls.Primitives;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class SwitchContentControl : TemplatedControl
{
    public static readonly DirectProperty<SwitchContentControl, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<SwitchContentControl, object?>(nameof(Value), o => o.Value, (o, v) => o.Value = v);

    private object? _value = default;

    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    void Test()
    {
    }
}