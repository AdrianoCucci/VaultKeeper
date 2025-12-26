using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using System;
using VaultKeeper.AvaloniaApplication.Abstractions;

namespace VaultKeeper.AvaloniaApplication.Services;

public class PlatformService(IApplicationLifetime applicationLifetime) : IPlatformService
{
    public IClipboard GetClipboard()
    {
        IClipboard? clipboard = null;

        if (applicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            clipboard = desktop.MainWindow?.Clipboard;

        return clipboard ?? throw new InvalidOperationException("Unable to access clipboard for the current platform state.");
    }
}
