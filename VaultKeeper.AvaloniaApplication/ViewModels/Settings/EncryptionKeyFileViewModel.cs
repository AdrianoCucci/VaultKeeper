using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VaultKeeper.AvaloniaApplication.Abstractions;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.ApplicationData.Files;
using VaultKeeper.Models.Errors;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.Security;

namespace VaultKeeper.AvaloniaApplication.ViewModels.Settings;

public partial class EncryptionKeyFileViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _filePath;

    private bool _hasExistingKey;
    public bool HasExistingKey { get => _hasExistingKey; private set => SetProperty(ref _hasExistingKey, value); }

    private bool _hasFilePath;
    public bool HasFilePath { get => _hasFilePath; private set => SetProperty(ref _hasFilePath, value); }

    private bool _isFilePathValid;
    public bool IsFilePathValid { get => _isFilePathValid; private set => SetProperty(ref _isFilePathValid, value); }

    private string? _filePathMessage;
    public string? FilePathMessage { get => _filePathMessage; private set => SetProperty(ref _filePathMessage, value); }

    private readonly IAppConfigService _appConfigService;
    private readonly IEncryptionService _encryptionService;
    private readonly IFileService _fileService;
    private readonly IAppFileDefinitionService _fileDefinitionService;
    private readonly IPlatformService _platformService;
    private readonly IErrorReportingService _errorReportingService;

    public EncryptionKeyFileViewModel(
        IAppConfigService appConfigService,
        IEncryptionService encryptionService,
        IFileService fileService,
        IAppFileDefinitionService fileDefinitionService,
        IPlatformService platformService,
        IErrorReportingService errorReportingService)
    {
        _appConfigService = appConfigService;
        _encryptionService = encryptionService;
        _fileService = fileService;
        _fileDefinitionService = fileDefinitionService;
        _platformService = platformService;
        _errorReportingService = errorReportingService;

        LoadFromSettings();
    }

#if DEBUG
#pragma warning disable CS8618
    public EncryptionKeyFileViewModel() { }
#pragma warning restore CS8618
#endif

    public void LoadFromSettings()
    {
        if (_appConfigService.GetConfigDataOrDefault() is AppConfigData appConfig)
        {
            FilePath = appConfig.EncryptionKeyPath;
            HasExistingKey = !string.IsNullOrWhiteSpace(appConfig.EncryptionKeyPath);
        }
        else
        {
            FilePath = null;
            HasExistingKey = false;
        }
    }

    public async Task GenerateKeyFileAsync()
    {
        FilePathMessage = null;

        IReadOnlyList<IStorageFolder> folders = await _platformService.OpenFolderPickerAsync(new()
        {
            Title = "Select Encryption Key File Directory",
            AllowMultiple = false
        });

        if (folders.Count < 1) return;

        string? folderPath = folders[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(folderPath)) return;

        Result canWriteResult = _fileService.CanWriteToDirectory(folderPath);
        if (!canWriteResult.IsSuccessful)
        {
            IsFilePathValid = false;
            FilePathMessage = canWriteResult.Message;
            return;
        }

        AppFileDefinition fileDefinition = _fileDefinitionService.GetFileDefinitionByType(AppFileType.EncryptionKey);
        string filePath = Path.Combine(folderPath, fileDefinition.FullName);

        if (_fileService.FileExists(filePath))
        {
            IsFilePathValid = false;
            FilePathMessage = "An existing encryption key file already exists at this directory - you must manually remove this file from this directory if you wish to generate a new key here.";
            return;
        }

        Result<string> generateFileDataResult = _encryptionService.GenerateEncryptionKeyFileData();
        if (!generateFileDataResult.IsSuccessful)
        {
            ReportError(generateFileDataResult, "Failed to Generate Encryption File Data");
            return;
        }

        Result writeFileResult = _fileService.WriteFileText(filePath, generateFileDataResult.Value!, FileAttributes.ReadOnly);
        if (!writeFileResult.IsSuccessful)
        {
            ReportError(writeFileResult, "Failed to Create Encryption Key File");
            return;
        }

        await TryUseEncryptionKeyFileAsync(filePath);
    }

    public async Task SelectKeyFileAsync()
    {
        FilePathMessage = null;

        AppFileDefinition fileDefinition = _fileDefinitionService.GetFileDefinitionByType(AppFileType.EncryptionKey);

        IReadOnlyList<IStorageFile> files = await _platformService.OpenFilePickerAsync(new()
        {
            Title = "Select Encryption Key File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new($"{fileDefinition.Name} Encryption Key File") { Patterns = [$"*{fileDefinition.Extension}"] }
            ]
        });

        if (files.Count < 1) return;

        string? filePath = files[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(filePath)) return;

        await TryUseEncryptionKeyFileAsync(filePath);
    }

    partial void OnFilePathChanged(string? value)
    {
        HasFilePath = !string.IsNullOrWhiteSpace(value);

        if (!HasFilePath)
        {
            IsFilePathValid = true;
            FilePathMessage = null;
        }
        else
        {
            Result keyFileValidResult = _encryptionService.VerifyValidEncryptionKeyFile(value);
            IsFilePathValid = keyFileValidResult.IsSuccessful;
            FilePathMessage = keyFileValidResult.IsSuccessful ? "Encryption key file path is OK." : keyFileValidResult.Message;
        }
    }

    private async Task<bool> TryUseEncryptionKeyFileAsync(string filePath)
    {
        var result = await _appConfigService.SetEncryptionKeyFilePathAsync(filePath);
        if (!result.IsSuccessful)
        {
            ReportError(result, "Failed to Use Encryption Key File");
            return false;
        }

        HasExistingKey = true;
        FilePath = filePath;

        return true;
    }

    private void ReportError(Result failedResult, string header)
    {
        bool isUserError = failedResult.FailureType != ResultFailureType.Unknown;

        _errorReportingService.ReportError(new()
        {
            Header = header,
            Message = $"({failedResult.FailureType}) - {failedResult.Message}",
            Exception = failedResult.Exception,
            Source = isUserError ? ErrorSource.User : ErrorSource.Application,
            Severity = isUserError ? ErrorSeverity.Normal : ErrorSeverity.High
        });
    }
}
