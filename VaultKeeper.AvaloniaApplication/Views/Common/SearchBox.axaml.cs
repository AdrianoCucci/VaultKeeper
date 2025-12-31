using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

[TemplatePart("PART_TextBox", typeof(TextBox))]
public class SearchBox : TemplatedControl
{
    public static readonly RoutedEvent<TextInputEventArgs> DebounceEvent =
        RoutedEvent.Register<TextInputEventArgs>(nameof(Debounce), RoutingStrategies.Bubble, typeof(SearchBox));

    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<SearchBox, string?>(
        nameof(Text),
        defaultValue: default,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> WatermarkProperty = AvaloniaProperty.Register<SearchBox, string?>(nameof(Watermark), "Search...");

    public static readonly StyledProperty<int> DebounceTimeProperty = AvaloniaProperty.Register<SearchBox, int>(nameof(DebounceTime), 250);

    public event EventHandler<TextInputEventArgs> Debounce { add => AddHandler(DebounceEvent, value); remove => RemoveHandler(DebounceEvent, value); }

    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string? Watermark { get => GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }
    public int DebounceTime { get => GetValue(DebounceTimeProperty); set => SetValue(DebounceTimeProperty, value); }

    private TextBox? _innerTextBox;
    private DispatcherTimer? _debounceTimer;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_innerTextBox != null)
            _innerTextBox.TextChanged -= TextBox_TextChanged;

        _innerTextBox = e.NameScope.Find<TextBox>("PART_TextBox");

        if (_innerTextBox != null)
            _innerTextBox.TextChanged += TextBox_TextChanged;
    }

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        DisposeDebounceTimer(_debounceTimer);
        _debounceTimer = StartNewDebounceTimer();
    }

    private void DebounceTimer_Tick(object? sender, EventArgs e)
    {
        DisposeDebounceTimer(_debounceTimer);

        TextInputEventArgs debounceArgs = new()
        {
            RoutedEvent = DebounceEvent,
            Source = this,
            Text = Text,
        };

        RaiseEvent(debounceArgs);
    }

    private DispatcherTimer StartNewDebounceTimer()
    {
        DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(DebounceTime) };
        timer.Tick += DebounceTimer_Tick;
        timer.Start();

        return timer;
    }

    private void DisposeDebounceTimer(DispatcherTimer? timer)
    {
        if (timer != null)
        {
            timer.Stop();
            timer.Tick -= DebounceTimer_Tick;
        }
    }
}