using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using VaultKeeper.AvaloniaApplication.Helpers;

namespace VaultKeeper.AvaloniaApplication.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected static T CreateControl<T>(Action<T> configAction) where T : Control, new() => ControlHelper.CreateControl(configAction);
}

public abstract class ViewModelBase<T>(T model) : ViewModelBase
{
    public T Model { get; } = model;
}