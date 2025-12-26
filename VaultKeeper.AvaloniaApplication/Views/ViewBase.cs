using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace VaultKeeper.AvaloniaApplication.Views;

public abstract class ViewBase<TModel> : UserControl where TModel : class
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
