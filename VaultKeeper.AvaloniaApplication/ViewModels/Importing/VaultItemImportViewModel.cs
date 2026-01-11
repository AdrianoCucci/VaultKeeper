using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.Errors;
using VaultKeeper.Models.Importing;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Importing;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Importing;

public partial class VaultItemImportViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ImportSource> _importSources;

    [ObservableProperty]
    private ImportSource? _selectedImportSource;

    [ObservableProperty]
    private string? _importFilePath;

    [ObservableProperty]
    private int _itemsGridRows = 1;

    private readonly IImportService? _importService;
    private readonly IPlatformService? _platformService;
    private readonly IErrorReportingService? _errorReportingService;

    public VaultItemImportViewModel(IImportService importService, IPlatformService platformService, IErrorReportingService errorReportingService)
    {
        _importService = importService;
        _platformService = platformService;
        _errorReportingService = errorReportingService;

        _importSources = [.. _importService.GetImportSources()];
        _selectedImportSource = _importSources.FirstOrDefault();
    }

    public VaultItemImportViewModel()
    {
        _importSources = [];
    }

    public async Task<bool> SelectAndProcessImportFileAsync()
    {
        if (SelectedImportSource is not ImportSource importSource || _importService == null || _platformService == null) return false;

        IReadOnlyList<IStorageFile> fileList = await _platformService.OpenFilePickerAsync(new()
        {
            FileTypeFilter =
            [
                new($"Import File ({importSource.Name})") { Patterns = [importSource.FileType] }
            ]
        });

        if (fileList.Count < 1) return false;

        string importFilePath = fileList[0].Path.LocalPath;
        Result importResult = await _importService.ProcessImportAsync(importSource.Type, importFilePath);

        if (!importResult.IsSuccessful)
        {
            bool isApplicationError = importResult.FailureType == ResultFailureType.Unknown;

            _errorReportingService?.ReportError(new()
            {
                Header = "Failed to Import",
                Message = $"({importResult.FailureType}) - {importResult.Message}".Replace("Vault Item", "Key"),
                Exception = importResult.Exception,
                Source = isApplicationError ? ErrorSource.Application : ErrorSource.User,
                Severity = isApplicationError ? ErrorSeverity.High : ErrorSeverity.Normal
            });
        }

        return importResult.IsSuccessful;
    }
}