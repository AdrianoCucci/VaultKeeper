using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using System;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Overlays;

public class OverlayPanel : TemplatedControl
{
    public static readonly RoutedEvent<RoutedEventArgs> OverlayOpenedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(OverlayOpened), RoutingStrategies.Bubble, typeof(OverlayPanel));

    public static readonly RoutedEvent<RoutedEventArgs> OverlayClosedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(OverlayClosed), RoutingStrategies.Bubble, typeof(OverlayPanel));


    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<OverlayPanel, object?>(nameof(Content));

    public static readonly StyledProperty<object?> OverlayContentProperty = AvaloniaProperty.Register<OverlayPanel, object?>(nameof(OverlayContent));

    public static readonly StyledProperty<IDataTemplate?> OverlayTemplateProperty = AvaloniaProperty.Register<OverlayPanel, IDataTemplate?>(nameof(OverlayTemplate));

    public static readonly StyledProperty<bool> IsOverlayVisibleProperty = AvaloniaProperty.Register<OverlayPanel, bool>(nameof(IsOverlayVisible), false, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IBrush?> OverlayBackdropProperty = AvaloniaProperty.Register<OverlayPanel, IBrush?>(nameof(OverlayBackdrop), new SolidColorBrush(Colors.Black));

    public static readonly StyledProperty<double> OverlayBackdropOpacityProperty = AvaloniaProperty.Register<OverlayPanel, double>(nameof(OverlayBackdropOpacity), 0.5);

    public static readonly StyledProperty<IBrush?> OverlayBackgroundProperty = AvaloniaProperty.Register<OverlayPanel, IBrush?>(nameof(OverlayBackground));

    public static readonly StyledProperty<HorizontalAlignment> OverlayHorizontalAlignmentProperty = AvaloniaProperty.Register<OverlayPanel, HorizontalAlignment>(nameof(OverlayHorizontalAlignment), HorizontalAlignment.Center);

    public static readonly StyledProperty<VerticalAlignment> OverlayVerticalAlignmentProperty = AvaloniaProperty.Register<OverlayPanel, VerticalAlignment>(nameof(OverlayHorizontalAlignment), VerticalAlignment.Center);

    public static readonly StyledProperty<Thickness> OverlayMarginProperty = AvaloniaProperty.Register<OverlayPanel, Thickness>(nameof(OverlayMargin), new(0));

    public static readonly StyledProperty<Thickness> OverlayPaddingProperty = AvaloniaProperty.Register<OverlayPanel, Thickness>(nameof(OverlayPadding), new(10));

    public static readonly StyledProperty<CornerRadius> OverlayCornerRadiusProperty = AvaloniaProperty.Register<OverlayPanel, CornerRadius>(nameof(OverlayCornerRadius), new(4));

    public static readonly StyledProperty<bool> CloseOnBackdropClickedProperty = AvaloniaProperty.Register<OverlayPanel, bool>(nameof(CloseOnBackdropClicked));


    public event EventHandler<RoutedEventArgs> OverlayOpened { add => AddHandler(OverlayOpenedEvent, value); remove => RemoveHandler(OverlayOpenedEvent, value); }

    public event EventHandler<RoutedEventArgs> OverlayClosed { add => AddHandler(OverlayClosedEvent, value); remove => RemoveHandler(OverlayClosedEvent, value); }


    [Content]
    public object? Content { get => GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public object? OverlayContent { get => GetValue(OverlayContentProperty); set => SetValue(OverlayContentProperty, value); }
    public IDataTemplate? OverlayTemplate { get => GetValue(OverlayTemplateProperty); set => SetValue(OverlayTemplateProperty, value); }
    public IBrush? OverlayBackdrop { get => GetValue(OverlayBackdropProperty); set => SetValue(OverlayBackdropProperty, value); }
    public double OverlayBackdropOpacity { get => GetValue(OverlayBackdropOpacityProperty); set => SetValue(OverlayBackdropOpacityProperty, value); }
    public IBrush? OverlayBackground { get => GetValue(OverlayBackgroundProperty); set => SetValue(OverlayBackgroundProperty, value); }
    public HorizontalAlignment OverlayHorizontalAlignment { get => GetValue(OverlayHorizontalAlignmentProperty); set => SetValue(OverlayHorizontalAlignmentProperty, value); }
    public VerticalAlignment OverlayVerticalAlignment { get => GetValue(OverlayVerticalAlignmentProperty); set => SetValue(OverlayVerticalAlignmentProperty, value); }
    public Thickness OverlayMargin { get => GetValue(OverlayMarginProperty); set => SetValue(OverlayMarginProperty, value); }
    public Thickness OverlayPadding { get => GetValue(OverlayPaddingProperty); set => SetValue(OverlayPaddingProperty, value); }
    public CornerRadius OverlayCornerRadius { get => GetValue(OverlayCornerRadiusProperty); set => SetValue(OverlayCornerRadiusProperty, value); }
    public bool CloseOnBackdropClicked { get => GetValue(CloseOnBackdropClickedProperty); set => SetValue(CloseOnBackdropClickedProperty, value); }

    public bool IsOverlayVisible
    {
        get => GetValue(IsOverlayVisibleProperty);
        set
        {
            bool originalValue = GetValue(IsOverlayVisibleProperty);

            SetValue(IsOverlayVisibleProperty, value);

            if (value != originalValue)
            {
                if (value)
                    RaiseEvent(new(OverlayOpenedEvent, this));
                else
                    RaiseEvent(new(OverlayClosedEvent, this));
            }
        }
    }

    private Border? _backdrop;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_backdrop != null)
            _backdrop.PointerPressed -= Backdrop_PointerPressed;

        _backdrop = e.NameScope.Find<Border>("PART_Backdrop")!;
        _backdrop.PointerPressed += Backdrop_PointerPressed;
    }

    private void Backdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (CloseOnBackdropClicked)
            IsOverlayVisible = false;
    }
}