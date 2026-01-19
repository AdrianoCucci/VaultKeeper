using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using VaultKeeper.AvaloniaApplication.Extensions;

namespace VaultKeeper.AvaloniaApplication.Views.Common;

public class PageLayoutPanel : ContentControl
{
    private ScrollViewer? _scrollViewer;

    public void ScrollToTop() => _scrollViewer?.ScrollToTop();

    public void MoveScrollViewerPosition(KeyEventArgs e)
    {
        if (_scrollViewer == null) return;

        switch (e.Key)
        {
            case Key.Up:
                _scrollViewer.ScrollUp();
                e.Handled = true;
                _scrollViewer.Focus();
                break;
            case Key.Down:
                _scrollViewer.ScrollDown();
                e.Handled = true;
                _scrollViewer.Focus();
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