using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VaultKeeper.AvaloniaApplication.Forms.Common;

public abstract class Form(FormMode mode = FormMode.New) : ObservableValidator
{
    // Do not change method signature - is called by CommunityToolkit.Mvvm [CustomValidation] attribute.
    public static ValidationResult? ValidateExternalErrors(string? _, ValidationContext context)
    {
        Form instance = (Form)context.ObjectInstance;
        if (instance._externalErrors.TryGetValue(context.MemberName!, out string? error))
            return new ValidationResult(error);

        return ValidationResult.Success;
    }

    public FormMode Mode { get; set; } = mode;

    private readonly Dictionary<string, string> _externalErrors = [];

    public bool Validate()
    {
        ClearErrors();
        ValidateAllProperties();

        return !HasErrors;
    }

    public void ClearErrors()
    {
        _externalErrors.Clear();
        base.ClearErrors();
    }

    public void SetExternalError(string propertyName, string error)
    {
        _externalErrors[propertyName] = error;
        OnExternalErrorsUpdated(_externalErrors);
    }

    protected virtual void OnExternalErrorsUpdated(Dictionary<string, string> externalErrors) { }
}

public abstract class Form<T>(FormMode mode = FormMode.New) : Form(mode) where T : class
{
    public abstract T GetModel();
}