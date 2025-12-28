using CommunityToolkit.Mvvm.ComponentModel;

namespace VaultKeeper.AvaloniaApplication.Forms.Common;

public abstract class Form(FormMode mode = FormMode.New) : ObservableValidator
{
    public FormMode Mode { get; set; } = mode;

    public bool Validate()
    {
        ValidateAllProperties();
        return !HasErrors;
    }

    public void ClearErrors() => base.ClearErrors();
}

public abstract class Form<T>(FormMode mode = FormMode.New) : Form(mode) where T : class
{
    public abstract T GetModel();
}