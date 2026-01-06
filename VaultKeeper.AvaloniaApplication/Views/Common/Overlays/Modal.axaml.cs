using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Overlays;

public class Modal : ContentControl
{
    public static readonly RoutedEvent<RoutedEventArgs> BackdropPressedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(BackdropPressed), RoutingStrategies.Bubble, typeof(Modal));

    public static readonly StyledProperty<IBrush?> BackdropBackgroundProperty = AvaloniaProperty.Register<Modal, IBrush?>(nameof(BackdropBackground), new SolidColorBrush(Colors.Black));

    public static readonly StyledProperty<double> BackdropOpacityProperty = AvaloniaProperty.Register<Modal, double>(nameof(BackdropOpacity), 0.5);

    public static readonly StyledProperty<IBrush?> CardBackgroundProperty = AvaloniaProperty.Register<Modal, IBrush?>(nameof(CardBackground));

    public static readonly StyledProperty<HorizontalAlignment> CardHorizontalAlignmentProperty = AvaloniaProperty.Register<Modal, HorizontalAlignment>(nameof(CardHorizontalAlignment), HorizontalAlignment.Center);

    public static readonly StyledProperty<VerticalAlignment> CardVerticalAlignmentProperty = AvaloniaProperty.Register<Modal, VerticalAlignment>(nameof(CardHorizontalAlignment), VerticalAlignment.Center);

    public static readonly StyledProperty<Thickness> CardMarginProperty = AvaloniaProperty.Register<Modal, Thickness>(nameof(CardMargin), new(0));

    public static readonly StyledProperty<Thickness> CardPaddingProperty = AvaloniaProperty.Register<Modal, Thickness>(nameof(CardPadding), new(10));

    public static readonly StyledProperty<CornerRadius> CardCornerRadiusProperty = AvaloniaProperty.Register<Modal, CornerRadius>(nameof(CardCornerRadius), new(4));

    public event EventHandler<RoutedEventArgs> BackdropPressed { add => AddHandler(BackdropPressedEvent, value); remove => RemoveHandler(BackdropPressedEvent, value); }

    public IBrush? BackdropBackground { get => GetValue(BackdropBackgroundProperty); set => SetValue(BackdropBackgroundProperty, value); }
    public double BackdropOpacity { get => GetValue(BackdropOpacityProperty); set => SetValue(BackdropOpacityProperty, value); }
    public IBrush? CardBackground { get => GetValue(CardBackgroundProperty); set => SetValue(CardBackgroundProperty, value); }
    public HorizontalAlignment CardHorizontalAlignment { get => GetValue(CardHorizontalAlignmentProperty); set => SetValue(CardHorizontalAlignmentProperty, value); }
    public VerticalAlignment CardVerticalAlignment { get => GetValue(CardVerticalAlignmentProperty); set => SetValue(CardVerticalAlignmentProperty, value); }
    public Thickness CardMargin { get => GetValue(CardMarginProperty); set => SetValue(CardMarginProperty, value); }
    public Thickness CardPadding { get => GetValue(CardPaddingProperty); set => SetValue(CardPaddingProperty, value); }
    public CornerRadius CardCornerRadius { get => GetValue(CardCornerRadiusProperty); set => SetValue(CardCornerRadiusProperty, value); }

    private Border? _backdrop;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_backdrop != null)
            _backdrop.PointerPressed -= Backdrop_PointerPressed;

        _backdrop = e.NameScope.Find<Border>("PART_Backdrop")!;
        _backdrop.PointerPressed += Backdrop_PointerPressed;
    }

    private void Backdrop_PointerPressed(object? sender, PointerPressedEventArgs e) => RaiseEvent(new(BackdropPressedEvent, this));
}