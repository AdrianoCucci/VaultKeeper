using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using System.Collections.Specialized;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class ListView : ItemsControl
{
    public static readonly StyledProperty<IDataTemplate?> EmptyTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(EmptyTemplate));

    public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
       AvaloniaProperty.Register<ListView, ScrollBarVisibility>(nameof(HorizontalScrollBarVisibility), ScrollBarVisibility.Auto);

    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<ListView, ScrollBarVisibility>(nameof(VerticalScrollBarVisibilityProperty), ScrollBarVisibility.Auto);

    public static readonly DirectProperty<ListView, bool> HasItemsProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(nameof(HasItems), o => o.HasItems);

    public static readonly DirectProperty<ListView, bool> IsEmptyProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(nameof(IsEmpty), o => o.IsEmpty);

    public IDataTemplate? EmptyTemplate { get => GetValue(EmptyTemplateProperty); set => SetValue(EmptyTemplateProperty, value); }
    public ScrollBarVisibility HorizontalScrollBarVisibility { get => GetValue(HorizontalScrollBarVisibilityProperty); set => SetValue(HorizontalScrollBarVisibilityProperty, value); }
    public ScrollBarVisibility VerticalScrollBarVisibility { get => GetValue(VerticalScrollBarVisibilityProperty); set => SetValue(VerticalScrollBarVisibilityProperty, value); }

    private bool _hasItems = false;
    public bool HasItems { get => _hasItems; private set => SetAndRaise(HasItemsProperty, ref _hasItems, value); }

    private bool _isEmpty = true;
    public bool IsEmpty { get => _isEmpty; private set => SetAndRaise(IsEmptyProperty, ref _isEmpty, value); }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UpdateHasItemsIsEmpty();
        Items.CollectionChanged += Items_CollectionChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) => Items.CollectionChanged -= Items_CollectionChanged;

    private void UpdateHasItemsIsEmpty()
    {
        HasItems = Items.Count > 0;
        IsEmpty = Items.Count < 1;
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateHasItemsIsEmpty();
}