using CommunityToolkit.Mvvm.ComponentModel;
using VaultKeeper.AvaloniaApplication.Models;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

/// <summary>
/// This is a ViewModel which represents a <see cref="TodoItem"/>
/// </summary>
public partial class TodoItemViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the checked status of each item
    /// </summary>
    [ObservableProperty]
    private bool _isChecked;

    /// <summary>
    /// Gets or sets the content of the to-do item
    /// </summary>
    [ObservableProperty]
    private string? _content;

    public TodoItemViewModel()
    {

    }

    public TodoItemViewModel(TodoItem toDoItem)
    {
        _isChecked = toDoItem.IsChecked;
        _content = toDoItem.Content;
    }

    /// <summary>
    /// Gets a TodoItem of this ViewModel
    /// </summary>
    /// <returns>The ToDoItem</returns>
    public TodoItem GetModel()
    {
        return new TodoItem()
        {
            IsChecked = IsChecked,
            Content = Content
        };
    }
}