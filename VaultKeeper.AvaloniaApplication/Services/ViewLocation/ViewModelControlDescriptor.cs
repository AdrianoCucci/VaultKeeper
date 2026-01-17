using Avalonia.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;

namespace VaultKeeper.AvaloniaApplication.Services.ViewLocation;

[method: SetsRequiredMembers]
public class ViewModelControlDescriptor(Type viewModelType, Type controlType) : IViewModelControlDescriptor
{
    public Type ViewModelType { get; } = viewModelType;
    public Type ControlType { get; } = controlType;

    public Control CreateControl()
    {
        Control control = InstantiateControl();
        control.DataContext = InstantiateViewModel();

        return control;
    }

    public Control CreateControl(object? viewModel)
    {
        Control control = InstantiateControl();
        control.DataContext = viewModel;

        return control;
    }

    public Control CreateControl(Func<object?> viewModelFactory)
    {
        Control control = InstantiateControl();
        control.DataContext = viewModelFactory.Invoke();

        return control;
    }

    private Control InstantiateControl() => (Control)Activator.CreateInstance(ControlType)!;

    private object? InstantiateViewModel() => Activator.CreateInstance(ViewModelType);
}
