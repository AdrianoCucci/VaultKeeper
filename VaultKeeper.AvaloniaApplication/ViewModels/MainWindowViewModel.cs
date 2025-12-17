using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using VaultKeeper.AvaloniaApplication.Constants;
using VaultKeeper.AvaloniaApplication.ViewModels.Common;
using VaultKeeper.AvaloniaApplication.ViewModels.VaultItems;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Repositories.Abstractions;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    public ObservableCollection<TodoItemViewModel> TodoItems { get; } = [];

    public ObservableCollection<NavItemViewModel> TabNavItems { get; } =
    [
        new(new()
        {
            Key = "Vault",
            NavContent = CreateControl<StackPanel>(panel =>
            {
                panel.Orientation = Avalonia.Layout.Orientation.Horizontal;
                panel.Children.Add(CreateControl<PathIcon>(x =>
                {
                    x.Margin = new(0, 0, 6, 0);
                    x.Bind(PathIcon.DataProperty, x.Resources.GetResourceObservable(Icons.Gear));
                }));
                panel.Children.Add(CreateControl<TextBlock>(x => x.Text = "Vault"));
            }),
            MainContent = new VaultItemListViewModel(Enumerable.Range(1, 10).Select(x => new VaultItem
            {
                Name = $"{x}: My Account",
                Value = $"{x}: Password123"
            }))
        }),
        new(new()
        {
            Key = "Settings",
            NavContent = CreateControl<StackPanel>(panel =>
            {
                panel.Orientation = Avalonia.Layout.Orientation.Horizontal;
                panel.Children.Add(CreateControl<PathIcon>(x =>
                {
                    x.Margin = new(0, 0, 6, 0);
                    x.Bind(PathIcon.DataProperty, x.Resources.GetResourceObservable(Icons.Gear));
                }));
                panel.Children.Add(CreateControl<TextBlock>(x => x.Text = "Settings"));
            }),
            MainContent = new VaultItemViewModel(new()
            {
                Name = "My Account",
                Value = "Password123"
            })
        }),
    ];

    private readonly IRepository<VaultItem> _vaultItemRepository;

    [ObservableProperty]
    private NavItemViewModel _selectedTab;

    /// <summary>
    /// Gets or set the content for new Items to add. If this string is not empty, the AddItemCommand will be enabled automatically
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddItemCommand))] // This attribute will invalidate the command each time this property changes
    private string? _newItemContent;

    public MainWindowViewModel(IRepository<VaultItem> vaultItemRepository)
    {
        _selectedTab = TabNavItems[0];
        _vaultItemRepository = vaultItemRepository;
    }

    /// <summary>
    /// Returns if a new Item can be added. We require to have the NewItem some Text
    /// </summary>
    private bool CanAddItem() => !string.IsNullOrWhiteSpace(NewItemContent);

    /// <summary>
    /// This command is used to add a new Item to the List
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddItem))]
    private void AddItem()
    {
        // Add a new item to the list
        TodoItems.Add(new TodoItemViewModel() { Content = NewItemContent });

        // reset the NewItemContent
        NewItemContent = null;
    }

    /// <summary>
    /// Removes the given Item from the list
    /// </summary>
    /// <param name="item">the item to remove</param>
    [RelayCommand]
    private void RemoveItem(TodoItemViewModel item)
    {
        // Remove the given item from the list
        TodoItems.Remove(item);
    }
}
