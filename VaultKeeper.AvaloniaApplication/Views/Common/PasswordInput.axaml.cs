using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using VaultKeeper.AvaloniaApplication.Extensions;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

[PseudoClasses(":empty", ":revealed")]
public class PasswordInput : TemplatedControl
{
    public static readonly RoutedEvent<RoutedEventArgs> ToggleRevealTextClickedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(ToggleRevealTextClicked), RoutingStrategies.Bubble, typeof(PasswordInput));

    public static readonly RoutedEvent<RoutedEventArgs> TextRevealedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(TextRevealed), RoutingStrategies.Bubble, typeof(PasswordInput));

    public static readonly RoutedEvent<RoutedEventArgs> TextHiddenEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(TextHidden), RoutingStrategies.Bubble, typeof(PasswordInput));

    public static readonly StyledProperty<string?> LabelProperty = AvaloniaProperty.Register<PasswordInput, string?>(nameof(Label));

    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<SearchBox, string?>(
        nameof(Text),
        defaultValue: default,
        defaultBindingMode: BindingMode.TwoWay,
        enableDataValidation: true);

    public static readonly StyledProperty<double> TextBoxMinHeightProperty = AvaloniaProperty.Register<PasswordInput, double>(nameof(TextBoxMinHeight), double.NaN);

    public static readonly StyledProperty<double> TextBoxMaxHeightProperty = AvaloniaProperty.Register<PasswordInput, double>(nameof(TextBoxMaxHeight), double.NaN);

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty = AvaloniaProperty.Register<PasswordInput, TextAlignment>(nameof(TextAlignment));

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty = AvaloniaProperty.Register<PasswordInput, HorizontalAlignment>(nameof(HorizontalContentAlignment));

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty = AvaloniaProperty.Register<PasswordInput, VerticalAlignment>(nameof(VerticalContentAlignment));

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty = AvaloniaProperty.Register<PasswordInput, TextWrapping>(nameof(TextWrapping));

    public static readonly StyledProperty<bool> AcceptsReturnProperty = AvaloniaProperty.Register<PasswordInput, bool>(nameof(AcceptsReturn));

    public static readonly StyledProperty<bool> IsRevealedProperty = AvaloniaProperty.Register<PasswordInput, bool>(nameof(IsRevealed));

    public static readonly StyledProperty<bool> AutoToggleRevealProperty = AvaloniaProperty.Register<PasswordInput, bool>(nameof(AutoToggleReveal), true);

    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<PasswordInput, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<object?> InnerRightContentProperty = AvaloniaProperty.Register<PasswordInput, object?>(nameof(InnerRightContent));

    public static readonly DirectProperty<PasswordInput, char> PasswordCharProperty = AvaloniaProperty
        .RegisterDirect<PasswordInput, char>(nameof(PasswordChar), o => o.PasswordChar);


    public event EventHandler<RoutedEventArgs> ToggleRevealTextClicked
    {
        add => AddHandler(ToggleRevealTextClickedEvent, value); remove => RemoveHandler(ToggleRevealTextClickedEvent, value);
    }
    public event EventHandler<RoutedEventArgs> TextRevealed
    {
        add => AddHandler(TextRevealedEvent, value); remove => RemoveHandler(TextRevealedEvent, value);
    }
    public event EventHandler<RoutedEventArgs> TextHidden
    {
        add => AddHandler(TextHiddenEvent, value); remove => RemoveHandler(TextHiddenEvent, value);
    }

    public string? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string? Text { get => GetValue(TextProperty); set => SetText(value); }
    public double TextBoxMinHeight { get => GetValue(TextBoxMinHeightProperty); set => SetValue(TextBoxMinHeightProperty, value); }
    public double TextBoxMaxHeight { get => GetValue(TextBoxMaxHeightProperty); set => SetValue(TextBoxMaxHeightProperty, value); }
    public TextAlignment TextAlignment { get => GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public HorizontalAlignment HorizontalContentAlignment { get => GetValue(HorizontalContentAlignmentProperty); set => SetValue(HorizontalContentAlignmentProperty, value); }
    public VerticalAlignment VerticalContentAlignment { get => GetValue(VerticalContentAlignmentProperty); set => SetValue(VerticalContentAlignmentProperty, value); }
    public TextWrapping TextWrapping { get => GetValue(TextWrappingProperty); set => SetValue(TextWrappingProperty, value); }
    public bool AcceptsReturn { get => GetValue(AcceptsReturnProperty); set => SetValue(AcceptsReturnProperty, value); }
    public bool IsRevealed { get => GetValue(IsRevealedProperty); set => SetValue(IsRevealedProperty, value); }
    public bool AutoToggleReveal { get => GetValue(AutoToggleRevealProperty); set => SetValue(AutoToggleRevealProperty, value); }
    public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }
    public object? InnerRightContent { get => GetValue(InnerRightContentProperty); set => SetValue(InnerRightContentProperty, value); }

    private char _passwordChar = '\0';
    public char PasswordChar { get => _passwordChar; private set => SetAndRaise(PasswordCharProperty, ref _passwordChar, value); }

    private TextBox? _textBox;
    private AppButton? _toggleRevealValueButton;

    public new bool Focus(NavigationMethod method = NavigationMethod.Unspecified, KeyModifiers keyModifiers = KeyModifiers.None) =>
        _textBox?.Focus(method, keyModifiers) == true;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textBox = e.NameScope.Find<TextBox>("PART_TextBox")!;

        if (_toggleRevealValueButton != null)
            _toggleRevealValueButton.Click -= ToggleRevealValueButton_Click;

        _toggleRevealValueButton = e.NameScope.Find<AppButton>("PART_ToggleRevealValueButton")!;
        _toggleRevealValueButton.Click += ToggleRevealValueButton_Click;

        RefreshUI();
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        if (property == TextProperty && _textBox != null)
            DataValidationErrors.SetError(_textBox, error);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == nameof(IsRevealed))
        {
            RefreshUI();

            if (IsRevealed)
                RaiseEvent(new(TextRevealedEvent, this));
            else
                RaiseEvent(new(TextHiddenEvent, this));
        }
    }

    private void SetText(string? text)
    {
        SetValue(TextProperty, text);
        UpdateEmptyPseudoClass();
    }

    private void RefreshUI()
    {
        UpdateToggleButtonUI();
        UpdatePasswordChar();
        UpdateEmptyPseudoClass();
        UpdateRevealedPseudoClass();
    }

    private void UpdateToggleButtonUI()
    {
        if (_toggleRevealValueButton == null) return;

        Application? application = Application.Current;
        if (application == null) return;

        if (IsRevealed)
        {
            _toggleRevealValueButton.IconStart = application.GetResourceOrDefault<Geometry>("IconEyeSlash");
            ToolTip.SetTip(_toggleRevealValueButton, "Hide value");
        }
        else
        {
            _toggleRevealValueButton.IconStart = application.GetResourceOrDefault<Geometry>("IconEye");
            ToolTip.SetTip(_toggleRevealValueButton, "Show value");
        }
    }

    private void UpdatePasswordChar() => PasswordChar = IsRevealed ? '\0' : '*';

    private void UpdateEmptyPseudoClass() => PseudoClasses.Set(":empty", string.IsNullOrWhiteSpace(Text));

    private void UpdateRevealedPseudoClass() => PseudoClasses.Set(":revealed", IsRevealed);

    private void ToggleRevealValueButton_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new(ToggleRevealTextClickedEvent, this));

        if (AutoToggleReveal)
            IsRevealed = !IsRevealed;
    }
}