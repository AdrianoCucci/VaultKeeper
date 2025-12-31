using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System.Collections.Specialized;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class ListView : ItemsControl
{
    public static readonly StyledProperty<IDataTemplate?> EmptyTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(nameof(EmptyTemplate));

    public static readonly DirectProperty<ListView, bool> HasItemsProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(nameof(HasItems), o => o.HasItems);

    public static readonly DirectProperty<ListView, bool> IsEmptyProperty =
        AvaloniaProperty.RegisterDirect<ListView, bool>(nameof(IsEmpty), o => o.IsEmpty);

    public IDataTemplate? EmptyTemplate { get => GetValue(EmptyTemplateProperty); set => SetValue(EmptyTemplateProperty, value); }

    private bool _hasItems = false;
    public bool HasItems { get => _hasItems; private set => SetAndRaise(HasItemsProperty, ref _hasItems, value); }

    private bool _isEmpty = true;
    public bool IsEmpty { get => _isEmpty; private set => SetAndRaise(IsEmptyProperty, ref _isEmpty, value); }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Update();
        Items.CollectionChanged += Items_CollectionChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) => Items.CollectionChanged -= Items_CollectionChanged;

    private void Update()
    {
        bool hasItems = Items.Count > 0;
        HasItems = hasItems;
        IsEmpty = !hasItems;
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Update();
}