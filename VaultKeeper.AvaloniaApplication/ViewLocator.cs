using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using System;
using VaultKeeper.AvaloniaApplication.Abstractions.ViewLocation;
using VaultKeeper.AvaloniaApplication.ViewModels;

namespace VaultKeeper.AvaloniaApplication;

public class ViewLocator(IViewLocatorService viewLocatorService) : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param == null) return null;

        Type paramType = param.GetType();

        Control control = viewLocatorService.GetView(param) ?? new TextBlock
        {
            Text = $"[{nameof(ViewLocator)}] VIEW NOT FOUND: {paramType.Name}",
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Colors.Red)
        };

        return control;
    }

    public bool Match(object? data) => data is ViewModelBase;
}
