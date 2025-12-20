using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using VaultKeeper.AvaloniaApplication.Helpers;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected static T CreateControl<T>(Action<T> configAction) where T : Control, new() => ControlHelper.CreateControl(configAction);
}

public abstract partial class ViewModelBase<T>(T model) : ViewModelBase
{
    [ObservableProperty]
    private T _model = model;

    public void UpdateModel(Func<T, T> updateAction) => Model = updateAction.Invoke(Model);
}