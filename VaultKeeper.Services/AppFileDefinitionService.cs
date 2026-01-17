using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VaultKeeper.Models.ApplicationData.Files;
using VaultKeeper.Services.Abstractions;

namespace VaultKeeper.Services;

public class AppFileDefinitionService(ILogger<AppFileDefinitionService> logger) : IAppFileDefinitionService
{
    private const string _commonExtension = ".vk";
    private const string _dataExtension = ".dat";
    private const string _backupExtension = ".bak";
    private const string _encryptionKeyExtension = ".key";

    private static readonly Dictionary<AppFileType, AppFileDefinition> _fileDefinitionsDict = new()
    {
        {
            AppFileType.AppConfig,
            new()
            {
                FileType = AppFileType.AppConfig,
                Name = "AppConfig",
                Extension = $"{_commonExtension}"
            }
        },
        {
            AppFileType.User,
            new()
            {
                FileType = AppFileType.User,
                Name = "User",
                Extension = $"{_dataExtension}{_commonExtension}"
            }
        },
        {
            AppFileType.Entities,
            new()
            {
                FileType = AppFileType.Entities,
                Name = "Entities",
                Extension = $"{_dataExtension}{_commonExtension}"
            }
        },
        {
            AppFileType.Backup,
            new()
            {
                FileType = AppFileType.Backup,
                Name = "VaultKeeper",
                Extension = $"{_backupExtension}{_commonExtension}"
            }
        },
        {
            AppFileType.EncryptionKey,
            new()
            {
                FileType = AppFileType.EncryptionKey,
                Name = "VaultKeeper",
                Extension = $"{_encryptionKeyExtension}{_commonExtension}"
            }
        }
    };

    public AppFileDefinition GetFileDefinitionByType(AppFileType fileType)
    {
        logger.LogInformation($"{nameof(GetFileDefinitionByType)} | {nameof(fileType)}: {{fileType}}", fileType);
        return _fileDefinitionsDict[fileType];
    }
}
