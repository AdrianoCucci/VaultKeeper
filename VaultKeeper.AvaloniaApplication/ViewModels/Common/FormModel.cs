using CommunityToolkit.Mvvm.ComponentModel;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common;

public abstract class FormModel<T> : ObservableValidator where T : class
{
    public abstract T GetModel();
}
