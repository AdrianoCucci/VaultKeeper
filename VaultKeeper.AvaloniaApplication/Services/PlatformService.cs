using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;

namespace VaultKeeper.AvaloniaApplication.Services;

public class PlatformService(ILogger<PlatformService> logger, IApplicationService applicationService) : IPlatformService
{
    public IClipboard GetClipboard()
    {
        logger.LogInformation(nameof(GetClipboard));

        IClassicDesktopStyleApplicationLifetime? desktop = GetDesktopApplicationLifetime();
        IClipboard? clipboard = desktop?.MainWindow?.Clipboard;

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

    private IClassicDesktopStyleApplicationLifetime? GetDesktopApplicationLifetime()
    {
        Application application = applicationService.GetApplication();
        if (application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop;

        return null;
    }

    private IStorageProvider GetStorageProviderOrThrow()
    {
        IClassicDesktopStyleApplicationLifetime? desktop = GetDesktopApplicationLifetime();
        IStorageProvider? storageProvider = desktop?.MainWindow?.StorageProvider;

        if (storageProvider == null || !storageProvider.CanOpen)
            throw new InvalidOperationException("Unable to access storage provider for the current platform state.");

        return storageProvider;
    }
}
