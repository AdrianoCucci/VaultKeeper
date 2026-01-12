using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    public Action? ProcessSucceededAction { get; set; }

    public ExportData? ExportData { get; set; }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsExportMode))]
    private VaultItemImportViewMode _mode = VaultItemImportViewMode.Import;

    [ObservableProperty]
    private ObservableCollection<ImportSource> _sources = [];

    [ObservableProperty]
    private ImportSource? _selectedSource;

    private bool _isProcessing = false;
    public bool IsProcessing { get => _isProcessing; private set => SetProperty(ref _isProcessing, value); }

    public bool IsExportMode => Mode == VaultItemImportViewMode.Export;

    private readonly IImportService _importService;
    private readonly IPlatformService _platformService;
    private readonly IErrorReportingService _errorReportingService;

    public VaultItemImportViewModel(
        IImportService importService,
        IPlatformService platformService,
        IErrorReportingService errorReportingService)
    {
        _importService = importService;
        _platformService = platformService;
        _errorReportingService = errorReportingService;

        _sources = [.. _importService.GetImportSources()];
        _selectedSource = _sources.FirstOrDefault();
    }

#if DEBUG
    public VaultItemImportViewModel()
    {
        _importService = null!;
        _platformService = null!;
        _errorReportingService = null!;
    }
#endif

    public async Task<bool> SelectAndProcessFileAsync()
    {
        IsProcessing = true;

        bool isProcessSuccssful = Mode switch
        {
            VaultItemImportViewMode.Import => await RunImportAsync(),
            VaultItemImportViewMode.Export => await RunExportAsync(),
            _ => false
        };

        IsProcessing = false;

        if (isProcessSuccssful)
            ProcessSucceededAction?.Invoke();

        return isProcessSuccssful;
    }

    private async Task<bool> RunImportAsync()
    {
        if (SelectedSource is not ImportSource source) return false;

        IReadOnlyList<IStorageFile> fileList = await _platformService.OpenFilePickerAsync(new()
        {
            FileTypeFilter =
            [
                new($"Import File ({source.Name})") { Patterns = [$"*{source.FileType}"] }
            ]
        });

        if (fileList.Count < 1) return false;

        string filePath = fileList[0].Path.LocalPath;

        Result processResult = await _importService.ImportFromFileAsync(source.Type, filePath);
        if (!processResult.IsSuccessful)
        {
            ReportProcessError(processResult);
            return false;
        }

        return true;
    }

    private async Task<bool> RunExportAsync()
    {
        if (SelectedSource is not ImportSource source) return false;
        if (ExportData is not ExportData exportData)
        {
            InvalidOperationException exception = new("Application error - data to export has not been set.");
            ReportProcessError(Result.Failed(exception.Message, exception));
            return false;
        }

        IReadOnlyList<IStorageFolder> folderList = await _platformService.OpenFolderPickerAsync(new()
        {
            Title = "Export Data",
            AllowMultiple = false
        });

        if (folderList.Count < 1) return false;

        string folderPath = folderList[0].Path.LocalPath;

        string fileName = "VaultKeeper_keys";

        if (source.Type != ImportSourceType.Application)
            fileName += $"_{source.Type.ToString().ToLower()}";

        fileName += source.FileType;

        string filePath = Path.Combine(folderPath, fileName);

        Result processResult = await _importService.ExportToFileAsync(source.Type, filePath, exportData);
        if (!processResult.IsSuccessful)
        {
            ReportProcessError(processResult);
            return false;
        }

        return true;
    }

    private void ReportProcessError(Result processResult)
    {
        string modeLabel = IsExportMode ? "Export" : "Import";
        bool isApplicationError = processResult.FailureType == ResultFailureType.Unknown;

        _errorReportingService?.ReportError(new()
        {
            Header = $"Failed to {modeLabel} Data",
            Message = $"({processResult.FailureType}) - {processResult.Message}".Replace("Vault Item", "Key"),
            Exception = processResult.Exception,
            Source = isApplicationError ? ErrorSource.Application : ErrorSource.User,
            Severity = isApplicationError ? ErrorSeverity.High : ErrorSeverity.Normal
        });
    }
}