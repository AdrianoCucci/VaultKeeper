using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;

namespace VaultKeeper.AvaloniaApplication.Services;

public class PlatformService(ILogger<PlatformService> logger, IApplicationLifetime applicationLifetime) : IPlatformService
{
    public IClipboard GetClipboard()
    {
        logger.LogInformation(nameof(GetClipboard));

        IClipboard? clipboard = null;

        if (applicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            clipboard = desktop.MainWindow?.Clipboard;

        return clipboard ?? throw new InvalidOperationException("Unable to access clipboard for the current platform state.");
    }

    public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
    {
        logger.LogInformation(nameof(OpenFilePickerAsync));

        IStorageProvider storageProvider = GetStorageProviderOrThrow();
        return storageProvider.OpenFilePickerAsync(options);
    }

    public Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
    {
        logger.LogInformation(nameof(OpenFolderPickerAsync));

        IStorageProvider storageProvider = GetStorageProviderOrThrow();
        return storageProvider.OpenFolderPickerAsync(options);
    }

    private IStorageProvider GetStorageProviderOrThrow()
    {
        IStorageProvider? storageProvider = applicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow?.StorageProvider
            : null;

        if (storageProvider == null || !storageProvider.CanOpen)
            throw new InvalidOperationException("Unable to access storage provider for the current platform state.");

        return storageProvider;
    }
}
