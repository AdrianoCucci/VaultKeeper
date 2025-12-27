using CommunityToolkit.Mvvm.ComponentModel;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common.Forms;

public abstract class Form(FormMode mode = FormMode.New) : ObservableValidator
{
    public FormMode Mode { get; set; } = mode;

    public bool Validate()
    {
        ValidateAllProperties();
        return !HasErrors;
    }
}

public abstract class Form<T>(FormMode mode = FormMode.New) : Form(mode) where T : class
{
    public abstract T GetModel();
}