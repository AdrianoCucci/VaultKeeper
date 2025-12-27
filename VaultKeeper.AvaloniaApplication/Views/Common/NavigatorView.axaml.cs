using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Models.Navigation;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class NavigatorView : ContentControl
{
    public static readonly DirectProperty<NavigatorView, INavigator?> NavigatorProperty =
        AvaloniaProperty.RegisterDirect<NavigatorView, INavigator?>(nameof(Navigator), o => o.Navigator, (o, v) => o.Navigator = v);

    public static readonly DirectProperty<NavigatorView, AvaloniaList<string>> ForRoutesProperty =
        AvaloniaProperty.RegisterDirect<NavigatorView, AvaloniaList<string>>(nameof(ForRoutes), o => o.ForRoutes);

    private INavigator? _navigator;
    private readonly AvaloniaList<string> _forRoutes = [];

    public INavigator? Navigator { get => _navigator; set => SetNavigator(value); }
    public AvaloniaList<string> ForRoutes => _forRoutes;

    private void SetNavigator(INavigator? navigator)
    {
        if (_navigator != null)
            _navigator.Navigated -= Navigator_Navigated;

        _navigator = navigator;
        if (_navigator == null)
        {
            Content = null;
        }
        else
        {
            UpdateNavigationContent(_navigator.CurrentRoute);
            _navigator.Navigated += Navigator_Navigated;
        }

        SetAndRaise(NavigatorProperty, ref _navigator, navigator);
    }

    private void UpdateNavigationContent(CurrentRoute currentRoute)
    {
        if (_forRoutes.IsNullOrEmpty() || _forRoutes.Contains(currentRoute.Key))
            Content = currentRoute.Content?.Invoke();
    }

    private void Navigator_Navigated(object? sender, CurrentRoute e) => UpdateNavigationContent(e);

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_navigator != null)
            _navigator.Navigated -= Navigator_Navigated;

        base.OnDetachedFromVisualTree(e);
    }
}