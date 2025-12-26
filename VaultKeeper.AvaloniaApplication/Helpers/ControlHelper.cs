using Avalonia.Controls;
using System;

namespace VaultKeeper.AvaloniaApplication.Helpers;

public static class ControlHelper
{
    public static T CreateControl<T>(Action<T> configAction) where T : Control, new()
    {
        T control = new();
        configAction.Invoke(control);

        return control;
    }
}
