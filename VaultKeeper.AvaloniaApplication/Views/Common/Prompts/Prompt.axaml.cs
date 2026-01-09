using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using System;

namespace VaultKeeper.AvaloniaApplication.Views.Common.Prompts;

public class Prompt : ContentControl
{
    public static readonly RoutedEvent<RoutedEventArgs> CloseButtonClickedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(CloseButtonClicked), RoutingStrategies.Bubble, typeof(Prompt));

    public static readonly RoutedEvent<RoutedEventArgs> OkButtonClickedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(OkButtonClicked), RoutingStrategies.Bubble, typeof(Prompt));

    public static readonly RoutedEvent<RoutedEventArgs> AcknowledgedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(Acknowledged), RoutingStrategies.Bubble, typeof(Prompt));

    public static readonly StyledProperty<string?> HeaderProperty = AvaloniaProperty.Register<Prompt, string?>(nameof(Header));

    public static readonly StyledProperty<string?> MessageProperty = AvaloniaProperty.Register<Prompt, string?>(nameof(Message));

    public static readonly StyledProperty<object?> FooterContentProperty = AvaloniaProperty.Register<Prompt, object?>(nameof(FooterContent));

    public static readonly StyledProperty<IDataTemplate?> FooterContentTemplateProperty = AvaloniaProperty.Register<Prompt, IDataTemplate?>(nameof(FooterContentTemplate));

    public static readonly StyledProperty<bool> ShowOkButtonProperty = AvaloniaProperty.Register<Prompt, bool>(nameof(ShowOkButton), true);

    public static readonly StyledProperty<double> ContentWidthProperty = AvaloniaProperty.Register<Prompt, double>(nameof(ContentWidth), double.NaN);

    public static readonly StyledProperty<double> ContentMinWidthProperty = AvaloniaProperty.Register<Prompt, double>(nameof(ContentMinWidth), double.NaN);

    public static readonly StyledProperty<double> ContentMaxWidthProperty = AvaloniaProperty.Register<Prompt, double>(nameof(ContentMaxWidth), double.NaN);

    public static readonly StyledProperty<double> ContentHeightProperty = AvaloniaProperty.Register<Prompt, double>(nameof(ContentHeight), double.NaN);

    public static readonly StyledProperty<double> ContentMinHeightProperty = AvaloniaProperty.Register<Prompt, double>(nameof(ContentMinHeight), double.NaN);

    public static readonly StyledProperty<double> ContentMaxHeightProperty = AvaloniaProperty.Register<Prompt, double>(nameof(ContentMaxHeight), double.NaN);

    public static readonly DirectProperty<Prompt, bool> HasContentProperty = AvaloniaProperty.RegisterDirect<Prompt, bool>(nameof(HasContent), o => o.HasContent);


    public event EventHandler<RoutedEventArgs> CloseButtonClicked { add => AddHandler(CloseButtonClickedEvent, value); remove => RemoveHandler(CloseButtonClickedEvent, value); }

    public event EventHandler<RoutedEventArgs> OkButtonClicked { add => AddHandler(OkButtonClickedEvent, value); remove => RemoveHandler(OkButtonClickedEvent, value); }

    public event EventHandler<RoutedEventArgs> Acknowledged { add => AddHandler(AcknowledgedEvent, value); remove => RemoveHandler(AcknowledgedEvent, value); }

    public string? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public string? Message { get => GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public object? FooterContent { get => GetValue(FooterContentProperty); set => SetValue(FooterContentProperty, value); }
    public IDataTemplate? FooterContentTemplate { get => GetValue(FooterContentTemplateProperty); set => SetValue(FooterContentTemplateProperty, value); }
    public bool ShowOkButton { get => GetValue(ShowOkButtonProperty); set => SetValue(ShowOkButtonProperty, value); }
    public double ContentWidth { get => GetValue(ContentWidthProperty); set => SetValue(ContentWidthProperty, value); }
    public double ContentMinWidth { get => GetValue(ContentMinWidthProperty); set => SetValue(ContentMinWidthProperty, value); }
    public double ContentMaxWidth { get => GetValue(ContentMaxWidthProperty); set => SetValue(ContentMaxWidthProperty, value); }
    public double ContentHeight { get => GetValue(ContentHeightProperty); set => SetValue(ContentHeightProperty, value); }
    public double ContentMinHeight { get => GetValue(ContentMinHeightProperty); set => SetValue(ContentMinHeightProperty, value); }
    public double ContentMaxHeight { get => GetValue(ContentMaxHeightProperty); set => SetValue(ContentMaxHeightProperty, value); }

    private bool _hasContent = default;
    public bool HasContent { get => _hasContent; private set => SetAndRaise(HasContentProperty, ref _hasContent, value); }

    private Button? _closeButton;
    private Button? _okButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateHasContent();

        if (_closeButton != null)
            _closeButton.Click -= CloseButton_Click;

        if (_okButton != null)
            _okButton.Click -= OkButton_Click;

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton")!;
        _okButton = e.NameScope.Find<Button>("PART_OkButton")!;

        _closeButton.Click += CloseButton_Click;
        _okButton.Click += OkButton_Click;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == nameof(Content))
            UpdateHasContent();
    }

    private void UpdateHasContent() => HasContent = Content != null;

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new(CloseButtonClickedEvent, this));
        RaiseEvent(new(AcknowledgedEvent, this));
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new(OkButtonClickedEvent, this));
        RaiseEvent(new(AcknowledgedEvent, this));
    }
}