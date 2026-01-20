using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using VaultKeeper.AvaloniaApplication.Extensions;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class PageLayoutPanel : ContentControl
{
    private ScrollViewer? _scrollViewer;

    public void ScrollToHome() => _scrollViewer?.ScrollToHome();

    public void MoveScrollViewerPosition(KeyEventArgs e)
    {
        if (_scrollViewer == null) return;

        void ExecuteScrollAction(Action action)
        {
            action.Invoke();
            e.Handled = true;
            _scrollViewer?.Focus();
        }

        switch (e.Key)
        {
            case Key.Up:
                ExecuteScrollAction(_scrollViewer.ScrollUp);
                break;
            case Key.Down:
                ExecuteScrollAction(_scrollViewer.ScrollDown);
                break;
            case Key.Home:
                ExecuteScrollAction(_scrollViewer.ScrollToHome);
                break;
            case Key.End:
                ExecuteScrollAction(_scrollViewer.ScrollToEnd);
                break;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_scrollViewer != null)
            _scrollViewer.KeyDown -= ScrollViewer_KeyDown;

        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer")!;
        _scrollViewer.KeyDown += ScrollViewer_KeyDown;
    }

    private void ScrollViewer_KeyDown(object? sender, KeyEventArgs e) => MoveScrollViewerPosition(e);
}