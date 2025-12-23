using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Common;

public abstract class Form<T>(FormMode mode = FormMode.New) : ObservableValidator where T : class
{
    public FormMode Mode { get; set; } = mode;

    private readonly Dictionary<FormAction, HashSet<Action>?> _actionHandlersDict = [];

    public bool Validate()
    {
        ValidateAllProperties();
        return !HasErrors;
    }

    public void On(FormAction action, Action handler)
    {
        if (_actionHandlersDict.TryGetValue(action, out HashSet<Action>? handlersSet))
        {
            handlersSet ??= [];
            handlersSet.Add(handler);
        }
        else
        {
            _actionHandlersDict[action] = [handler];
        }
    }

    public void Submit()
    {
        ValidateAllProperties();
        InvokeEventHandlers(FormAction.Submitted);
    }

    public void Cancel() => InvokeEventHandlers(FormAction.Cancelled);

    public abstract T GetModel();

    private void InvokeEventHandlers(FormAction action)
    {
        if (_actionHandlersDict.TryGetValue(action, out HashSet<Action>? handlersSet) && handlersSet != null)
        {
            foreach (var handler in handlersSet)
            {
                handler.Invoke();
            }
        }
    }
}