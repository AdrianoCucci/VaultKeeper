using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public abstract class ViewModelBase : ObservableObject;

public abstract partial class ViewModelBase<T>(T model) : ViewModelBase
{
    [ObservableProperty]
    private T _model = model;

    public void UpdateModel(Func<T, T> updateAction) => Model = updateAction.Invoke(Model);

    partial void OnModelChanging(T? oldValue, T newValue) => OnModelUpdating(oldValue, newValue);

    partial void OnModelChanged(T? oldValue, T newValue) => OnModelUpdated(oldValue, newValue);

    public virtual T GetUpdatedModel() => Model;

    protected virtual void OnModelUpdating(T? oldValue, T newValue) { }

    protected virtual void OnModelUpdated(T? oldValue, T newValue) { }
}