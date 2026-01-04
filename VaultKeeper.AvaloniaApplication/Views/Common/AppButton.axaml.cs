using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class AppButton : Button
{
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<AppButton, string?>(nameof(Text));

    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<AppButton, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<Geometry?> IconStartProperty = AvaloniaProperty.Register<AppButton, Geometry?>(nameof(IconStart));

    public static readonly StyledProperty<double> IconStartSizeProperty = AvaloniaProperty.Register<AppButton, double>(nameof(IconStartSize), 14);

    public static readonly StyledProperty<Geometry?> IconEndProperty = AvaloniaProperty.Register<AppButton, Geometry?>(nameof(IconEnd));

    public static readonly StyledProperty<double> IconEndSizeProperty = AvaloniaProperty.Register<AppButton, double>(nameof(IconEndSize), 14);

    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<AppButton, double>(nameof(Spacing), 12);


    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    public Geometry? IconStart { get => GetValue(IconStartProperty); set => SetValue(IconStartProperty, value); }
    public double? IconStartSize { get => GetValue(IconStartSizeProperty); set => SetValue(IconStartSizeProperty, value); }
    public Geometry? IconEnd { get => GetValue(IconEndProperty); set => SetValue(IconEndProperty, value); }
    public double? IconEndSize { get => GetValue(IconEndSizeProperty); set => SetValue(IconEndSizeProperty, value); }
    public double? Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }

    private Button? _innerButton;

    public AppButton() => Classes.CollectionChanged += Classes_CollectionChanged;

    ~AppButton() => Classes.CollectionChanged -= Classes_CollectionChanged;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _innerButton = e.NameScope.Find<Button>("PART_Button");
        UpdateInnerButtonClasses([], Classes);
    }

    private void UpdateInnerButtonClasses(IEnumerable<string> oldClasses, IEnumerable<string> newClasses)
    {
        if (_innerButton == null) return;

        IEnumerable<string> oldNonPseudoClasses = oldClasses.Where(x => !x.StartsWith(':'));
        IEnumerable<string> newNonPseudoClasses = newClasses.Where(x => !x.StartsWith(':'));

        _innerButton.Classes.RemoveAll(oldClasses);
        _innerButton.Classes.AddRange(newClasses);
    }

    private void Classes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IEnumerable<string> oldClasses = e.OldItems is IEnumerable<string> concreteOldItems ? concreteOldItems : [];
        IEnumerable<string> newClasses = e.NewItems is IEnumerable<string> concreteNewItems ? concreteNewItems : [];

        UpdateInnerButtonClasses(oldClasses, newClasses);
    }
}