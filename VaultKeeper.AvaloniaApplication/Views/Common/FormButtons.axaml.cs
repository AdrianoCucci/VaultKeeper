using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class FormButtons : TemplatedControl
{
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble, typeof(FormButtons));

    public static readonly RoutedEvent<RoutedEventArgs> SubmittedEvent =
        RoutedEvent.Register<RoutedEventArgs>(nameof(Submitted), RoutingStrategies.Bubble, typeof(FormButtons));

    public static readonly StyledProperty<string?> CancelButtonTextProperty = AvaloniaProperty.Register<FormButtons, string?>(nameof(CancelButtonText), "Cancel");

    public static readonly StyledProperty<Geometry?> CancelButtonIconProperty = AvaloniaProperty.Register<FormButtons, Geometry?>(nameof(CancelButtonIcon));

    public static readonly StyledProperty<string?> SubmitButtonTextProperty = AvaloniaProperty.Register<FormButtons, string?>(nameof(SubmitButtonText), "Submit");

    public static readonly StyledProperty<Geometry?> SubmitButtonIconProperty = AvaloniaProperty.Register<FormButtons, Geometry?>(nameof(SubmitButtonIcon));

    public static readonly StyledProperty<bool> IsSubmitButtonEnabledProperty = AvaloniaProperty.Register<FormButtons, bool>(nameof(IsSubmitButtonEnabled), true);


    public event EventHandler<RoutedEventArgs> Cancelled { add => AddHandler(CancelledEvent, value); remove => RemoveHandler(CancelledEvent, value); }

    public event EventHandler<RoutedEventArgs> Submitted { add => AddHandler(SubmittedEvent, value); remove => RemoveHandler(SubmittedEvent, value); }

    public string? CancelButtonText { get => GetValue(CancelButtonTextProperty); set => SetValue(CancelButtonTextProperty, value); }
    public Geometry? CancelButtonIcon { get => GetValue(CancelButtonIconProperty); set => SetValue(CancelButtonIconProperty, value); }
    public string? SubmitButtonText { get => GetValue(SubmitButtonTextProperty); set => SetValue(SubmitButtonTextProperty, value); }
    public Geometry? SubmitButtonIcon { get => GetValue(SubmitButtonIconProperty); set => SetValue(SubmitButtonIconProperty, value); }
    public bool IsSubmitButtonEnabled { get => GetValue(IsSubmitButtonEnabledProperty); set => SetValue(IsSubmitButtonEnabledProperty, value); }

    private Button? _cancelButton;
    private Button? _submitButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        SetupButton(ref _cancelButton, e.NameScope, "PART_CancelButton", ButtonCancel_Click);
        SetupButton(ref _submitButton, e.NameScope, "PART_SubmitButton", ButtonSubmit_Click);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DisposeButton(ref _cancelButton, ButtonCancel_Click);
        DisposeButton(ref _submitButton, ButtonSubmit_Click);
    }

    private static void SetupButton(ref Button? button, INameScope nameScope, string name, EventHandler<RoutedEventArgs> eventHandler)
    {
        DisposeButton(ref button, eventHandler);
        button = nameScope.Find<Button>(name);

        if (button != null)
            button.Click += eventHandler;
    }

    private static void DisposeButton(ref Button? button, EventHandler<RoutedEventArgs> eventHandler)
    {
        if (button != null)
            button.Click -= eventHandler;

        button = null;
    }

    private void ButtonCancel_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(CancelledEvent, sender));

    private void ButtonSubmit_Click(object? sender, RoutedEventArgs e) => RaiseEvent(new(SubmittedEvent, sender));
}