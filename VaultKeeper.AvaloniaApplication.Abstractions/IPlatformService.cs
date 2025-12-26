using Avalonia.Input.Platform;

namespace VaultKeeper.AvaloniaApplication.Abstractions;

public interface IPlatformService
{
    IClipboard GetClipboard();
}
