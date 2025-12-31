using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication.Views;

public abstract class ViewBase : UserControl;

public abstract class ViewBase<TModel> : ViewBase where TModel : ViewModelBase
{
    public TModel? Model => DataContext as TModel;

    public void UpdateModel(Action<TModel> action)
    {
        if (Model != null)
            action.Invoke(Model);
    }

    public async Task UpdateModelAsync(Func<TModel, Task> action)
    {
        if (Model != null)
            await action.Invoke(Model);
    }
}
