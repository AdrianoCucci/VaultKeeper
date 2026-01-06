using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using System;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Overlays;

public class OverlayPanel : ContentControl
{
    public static readonly RoutedEvent<RoutedEventArgs> OverlayOpenedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(OverlayOpened), RoutingStrategies.Bubble, typeof(OverlayPanel));

    public static readonly RoutedEvent<RoutedEventArgs> OverlayClosedEvent = RoutedEvent.Register<RoutedEventArgs>(nameof(OverlayClosed), RoutingStrategies.Bubble, typeof(OverlayPanel));


    public static readonly StyledProperty<object?> OverlayContentProperty = AvaloniaProperty.Register<OverlayPanel, object?>(nameof(OverlayContent));

    public static readonly StyledProperty<IDataTemplate?> OverlayContentTemplateProperty = AvaloniaProperty.Register<OverlayPanel, IDataTemplate?>(nameof(OverlayContentTemplate));

    public static readonly StyledProperty<bool> IsOverlayVisibleProperty = AvaloniaProperty.Register<OverlayPanel, bool>(nameof(IsOverlayVisible), false, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> OverlayPreventsMainContentInteractionProperty = AvaloniaProperty.Register<OverlayPanel, bool>(nameof(OverlayPreventsMainContentInteraction), true);

    public static readonly DirectProperty<OverlayPanel, bool> IsMainContentInteractionDisabledProperty =
        AvaloniaProperty.RegisterDirect<OverlayPanel, bool>(nameof(IsMainContentInteractionDisabled), o => o.IsMainContentInteractionDisabled);


    public event EventHandler<RoutedEventArgs> OverlayOpened { add => AddHandler(OverlayOpenedEvent, value); remove => RemoveHandler(OverlayOpenedEvent, value); }

    public event EventHandler<RoutedEventArgs> OverlayClosed { add => AddHandler(OverlayClosedEvent, value); remove => RemoveHandler(OverlayClosedEvent, value); }

    public object? OverlayContent { get => GetValue(OverlayContentProperty); set => SetValue(OverlayContentProperty, value); }
    public IDataTemplate? OverlayContentTemplate { get => GetValue(OverlayContentTemplateProperty); set => SetValue(OverlayContentTemplateProperty, value); }
    public bool IsOverlayVisible { get => GetValue(IsOverlayVisibleProperty); set => SetValue(IsOverlayVisibleProperty, value); }
    public bool OverlayPreventsMainContentInteraction { get => GetValue(OverlayPreventsMainContentInteractionProperty); set => SetValue(OverlayPreventsMainContentInteractionProperty, value); }


    private bool _isMainContentInteractionDisabled = default;
    public bool IsMainContentInteractionDisabled { get => _isMainContentInteractionDisabled; private set => SetAndRaise(IsMainContentInteractionDisabledProperty, ref _isMainContentInteractionDisabled, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateMainContentInteraction();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name != nameof(IsOverlayVisible)) return;

        if (change.GetNewValue<bool>())
            RaiseEvent(new(OverlayOpenedEvent, this));
        else
            RaiseEvent(new(OverlayClosedEvent, this));

        UpdateMainContentInteraction();
    }

    private void UpdateMainContentInteraction() =>
        IsMainContentInteractionDisabled = OverlayPreventsMainContentInteraction && IsOverlayVisible;
}