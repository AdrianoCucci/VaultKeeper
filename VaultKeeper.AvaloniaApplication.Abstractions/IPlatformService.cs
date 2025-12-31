using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaultKeeper.AvaloniaApplication.Abstractions;

public interface IPlatformService
{
    IClipboard GetClipboard();
    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options);
    Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options);
}
