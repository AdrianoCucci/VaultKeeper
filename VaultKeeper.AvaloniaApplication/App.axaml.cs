using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Services;
using VaultKeeper.AvaloniaApplication.ViewModels;
using VaultKeeper.AvaloniaApplication.Views;

namespace VaultKeeper.AvaloniaApplication;

public partial class App : Application
{
    // This is a reference to our MainViewModel which we use to save the list on shutdown. You can also use Dependency Injection
    // in your App.
    private readonly MainWindowViewModel _mainViewModel = new();

    private bool _canClose = false;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = _mainViewModel,
            };

            desktop.ShutdownRequested += Desktop_ShutdownRequestedAsync;
        }

        await InitMainViewModelAsync();

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    // Optional: Load data from disc
    private async Task InitMainViewModelAsync()
    {
        // get the items to load
        var itemsLoaded = await TodoListFileService.LoadFromFileAsync();

        if (itemsLoaded is not null)
        {
            _mainViewModel.TodoItems.Clear();

            foreach (var item in itemsLoaded)
            {
                _mainViewModel.TodoItems.Add(new TodoItemViewModel(item));
            }
        }
    }

    private async void Desktop_ShutdownRequestedAsync(object? sender, ShutdownRequestedEventArgs e)
    {
        e.Cancel = !_canClose; // cancel closing event first time

        if (!_canClose)
        {
            // To save the items, we map them to the ToDoItem-Model which is better suited for I/O operations
            var itemsToSave = _mainViewModel.TodoItems.Select(item => item.GetModel());
            await TodoListFileService.SaveToFileAsync(itemsToSave);

            // Set _canClose to true and Close this Window again
            _canClose = true;
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}