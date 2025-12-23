using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class AppButton : TemplatedControl
{

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble, typeof(AppButton));


    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<AppButton, string?>(nameof(Text));

    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<AppButton, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<Geometry?> IconStartProperty = AvaloniaProperty.Register<AppButton, Geometry?>(nameof(IconStart));

    public static readonly StyledProperty<double> IconStartSizeProperty = AvaloniaProperty.Register<AppButton, double>(nameof(IconStartSize), 14);

    public static readonly StyledProperty<Geometry?> IconEndProperty = AvaloniaProperty.Register<AppButton, Geometry?>(nameof(IconEnd));

    public static readonly StyledProperty<double> IconEndSizeProperty = AvaloniaProperty.Register<AppButton, double>(nameof(IconEndSize), 14);

    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<AppButton, double>(nameof(Spacing), 12);


    public event EventHandler<RoutedEventArgs> Click { add => AddHandler(ClickEvent, value); remove => RemoveHandler(ClickEvent, value); }

    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }

    public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }

    public Geometry? IconStart { get => GetValue(IconStartProperty); set => SetValue(IconStartProperty, value); }

    public double? IconStartSize { get => GetValue(IconStartSizeProperty); set => SetValue(IconStartSizeProperty, value); }

    public Geometry? IconEnd { get => GetValue(IconEndProperty); set => SetValue(IconEndProperty, value); }

    public double? IconEndSize { get => GetValue(IconEndSizeProperty); set => SetValue(IconEndSizeProperty, value); }

    public double? Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }

    private Button? _button;

    public AppButton()
    {
        MinHeight = 34;
        Padding = new(8);
        HorizontalAlignment = HorizontalAlignment.Left;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_button != null)
            _button.Click -= Button_Click;

        _button = e.NameScope.Find<Button>("PART_Button");

        if (_button != null)
            _button.Click += Button_Click;
    }

    private void Button_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(ClickEvent, this));
}