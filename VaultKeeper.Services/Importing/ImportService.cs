using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VaultKeeper.Common.Extensions;
using VaultKeeper.Common.Models.Queries;
using VaultKeeper.Common.Results;
using VaultKeeper.Models.ApplicationData;
using VaultKeeper.Models.Constants;
using VaultKeeper.Models.Groups;
using VaultKeeper.Models.Importing;
using VaultKeeper.Models.Security;
using VaultKeeper.Models.VaultItems;
using VaultKeeper.Services.Abstractions;
using VaultKeeper.Services.Abstractions.DataFormatting;
using VaultKeeper.Services.Abstractions.Groups;
using VaultKeeper.Services.Abstractions.Importing;
using VaultKeeper.Services.Abstractions.VaultItems;

namespace VaultKeeper.Services.Importing;

public class ImportService(
    ILogger<ImportService> logger,
    IFileService fileService,
    ICsvService csvService,
    IGroupService groupService,
    IVaultItemService vaultItemService,
    ISecurityService securityService,
    IAppDataService appDataService) : IImportService
{
    private static ImportSource[] Sources =>
    [
        new()
        {
            Type = ImportSourceType.Application,
            Name = "Vault Keeper",
            FileType = ".csv",
            Icon = Icons.VaultKeeper,
            Description = "A Vault Keeper keys CSV file."
        },
        new()
        {
            Type = ImportSourceType.Chromium,
            Name = "Chrome/Chromium",
            FileType = ".csv",
            Icon = Icons.GoogleChrome,
            Description = "A Google Chrome or other Chromium-based browser password CSV file.",
            AdditionalIcons = [Icons.MicrosoftEdge, Icons.BraveBrowser, Icons.OperaBrowser]
        },
        new()
        {
            Type = ImportSourceType.Firefox,
            Name = "Firefox",
            FileType = ".csv",
            Icon = Icons.MozillaFirefox,
            Description = "A Mozilla Firefox password CSV file."
        }
    ];

    public IEnumerable<ImportSource> GetImportSources()
    {
        logger.LogInformation(nameof(GetImportSources));
        return Sources;
    }

    public async Task<Result> ImportFromFileAsync(ImportSourceType sourceType, string sourceFilePath)
    {
        logger.LogInformation($"{nameof(ImportFromFileAsync)} | {nameof(sourceType)}: {{sourceType}}, {nameof(sourceFilePath)}: \"{{sourceFilePath}}\"", sourceType, sourceFilePath);

        Result<string> readFileResult = fileService.ReadFileText(sourceFilePath);
        if (!readFileResult.IsSuccessful)
            return readFileResult.Logged(logger);

        string? fileText = readFileResult.Value;
        if (string.IsNullOrWhiteSpace(fileText))
            return Result.Failed(ResultFailureType.BadRequest, "File contains no data to import.");

        Result<IEnumerable<VaultItemImportRecord>> deserializeResult = DeserializeImportFileText(sourceType, fileText);
        if (!deserializeResult.IsSuccessful)
            return deserializeResult.Logged(logger);

        VaultItemImportRecord[] importRecords = [.. deserializeResult.Value!];
        if (importRecords.Length < 1)
            return Result.Failed(ResultFailureType.BadRequest, "File contains no data to import.");

        VaultItemImportRecord[] duplicateImportNameRecords = [.. importRecords.DuplicatesBy(x => new { x.Name, x.GroupName })];
        if (duplicateImportNameRecords.Length > 0)
        {
            IEnumerable<string> duplicateLines = duplicateImportNameRecords
                .Select(x => $"- (Group: \"{x.GroupName}\", Name: \"{x.Name}\")")
                .Distinct();

            return Result.Failed(ResultFailureType.BadRequest, $"File contains rows with duplicate names:\n{string.Join("\n- ", duplicateLines)}");
        }

        Result<CountedData<VaultItem>> getExistingVaultItemsResult = await vaultItemService.GetManyCountedAsync();
        if (!getExistingVaultItemsResult.IsSuccessful)
            return getExistingVaultItemsResult.Logged(logger);

        Result<CountedData<Group>> getExistingGroupsResult = await groupService.GetManyCountedAsync();
        if (!getExistingGroupsResult.IsSuccessful)
            return getExistingGroupsResult.Logged(logger);

        HashSet<VaultItem> existingVaultItems = [.. getExistingVaultItemsResult.Value!.Items];
        IEnumerable<string> existingVaultItemNames = existingVaultItems.Select(x => x.Name);

        HashSet<Group> existingGroups = [.. getExistingGroupsResult.Value!.Items];
        IEnumerable<string> existingGroupNames = existingGroups.Select(x => x.Name);

        NewGroup[] groupsToCreate = [.. importRecords
            .Select(x => x.GroupName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Except(existingGroupNames)
            .Select(x => new NewGroup { Name = x })];

        if (groupsToCreate.Length > 0)
        {
            Result<IEnumerable<Group>> createGroupsResult = await groupService.AddManyAsync(groupsToCreate);
            if (!createGroupsResult.IsSuccessful)
                return createGroupsResult.Logged(logger);

            existingGroups.UnionWith(createGroupsResult.Value!);
        }

        List<NewVaultItem> vaultItemsToCreate = [];

        Result encryptResult = securityService.UsingEncryptionScope(scope =>
        {
            foreach (VaultItemImportRecord importRecord in importRecords)
            {
                vaultItemsToCreate.Add(new()
                {
                    Name = importRecord.Name,
                    Value = scope.Encrypt(importRecord.Value),
                    GroupId = existingGroups.FirstOrDefault(g => g.Name == importRecord.GroupName)?.Id
                });
            }
        });

        if (!encryptResult.IsSuccessful)
            return encryptResult.Logged(logger);

        Result<IEnumerable<VaultItem>> createVaultItemsResult = await vaultItemService.AddManyAsync(vaultItemsToCreate);
        if (!createVaultItemsResult.IsSuccessful)
            return createVaultItemsResult.Logged(logger);

        Result<SavedData<EntityData>?> saveDataResult = await appDataService.SaveEntityDataAsync();
        if (!saveDataResult.IsSuccessful)
            return saveDataResult.Logged(logger);

        return Result.Ok($"Imported {importRecords.Length} records successfully.").Logged(logger);
    }

    public async Task<Result> ExportToFileAsync(ImportSourceType sourceType, string filePath, ExportData exportData)
    {
        logger.LogInformation($"{nameof(ExportToFileAsync)} | {nameof(sourceType)}: {{sourceType}}", sourceType);

        VaultItemImportRecord[] records = [.. MapToApplicationRecords(exportData)];

        Result decryptResult = securityService.UsingEncryptionScope(scope =>
        {
            foreach (VaultItemImportRecord record in records)
            {
                record.Value = scope.Decrypt(record.Value);
            }
        });

        if (!decryptResult.IsSuccessful)
            return decryptResult.Logged(logger);

        Result<string> serializeResult;

        switch (sourceType)
        {
            case ImportSourceType.Application:
                {
                    serializeResult = csvService.Serialize(records);
                    break;
                }
            case ImportSourceType.Chromium:
            case ImportSourceType.Firefox:
                {
                    IEnumerable<BrowserImportRecord> browserRecords = MapToBrowserRecords(records);
                    serializeResult = csvService.Serialize(browserRecords);
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(sourceType), $"Value is not a valid {nameof(ImportSourceType)} enum value.");
        }

        if (!serializeResult.IsSuccessful)
            return serializeResult.Logged(logger);

        Result writeFileResult = fileService.WriteFileText(filePath, serializeResult.Value!);

        return writeFileResult.Logged(logger);
    }

    private Result<IEnumerable<VaultItemImportRecord>> DeserializeImportFileText(ImportSourceType sourceType, string fileText)
    {
        logger.LogInformation($"{nameof(DeserializeImportFileText)} | {nameof(sourceType)}: {{sourceType}}", sourceType);

        Result<IEnumerable<VaultItemImportRecord>> result;

        switch (sourceType)
        {
            case ImportSourceType.Application:
                result = csvService.Deserialize<VaultItemImportRecord>(fileText);
                break;
            case ImportSourceType.Chromium:
            case ImportSourceType.Firefox:
                {
                    Result<IEnumerable<BrowserImportRecord>> deserializeResult = csvService.Deserialize<BrowserImportRecord>(fileText);
                    if (!deserializeResult.IsSuccessful)
                    {
                        result = deserializeResult.WithValue<IEnumerable<VaultItemImportRecord>>();
                        break;
                    }

                    IEnumerable<VaultItemImportRecord> mappedRecords = MapToApplicationRecords(deserializeResult.Value!);
                    result = mappedRecords.ToOkResult();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sourceType), $"Value is not a valid {nameof(ImportSourceType)} enum value.");
        }

        return result.Logged(logger);
    }

    private static IEnumerable<VaultItemImportRecord> MapToApplicationRecords(ExportData exportData) =>
        exportData.VaultItems.Select(v => new VaultItemImportRecord
        {
            Name = v.Name,
            Value = v.Value,
            GroupName = exportData.Groups.FirstOrDefault(g => g.Id == v.GroupId)?.Name ?? string.Empty
        });

    private static IEnumerable<VaultItemImportRecord> MapToApplicationRecords(IEnumerable<BrowserImportRecord> browserRecords) =>
        browserRecords.Select(x =>
        {
            string name = x.Username;
            bool nameIsUrl = false;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = x.Url;
                nameIsUrl = true;
            }

            return new VaultItemImportRecord
            {
                Name = name,
                Value = x.Password,
                GroupName = nameIsUrl ? string.Empty : x.Url
            };
        });

    private static IEnumerable<BrowserImportRecord> MapToBrowserRecords(IEnumerable<VaultItemImportRecord> applicationRecords) =>
        applicationRecords.Select(x =>
        {
            string url = x.GroupName;
            bool nameIsUrl = false;

            if (string.IsNullOrWhiteSpace(url))
            {
                url = x.Name;
                nameIsUrl = true;
            }

            return new BrowserImportRecord
            {
                Url = nameIsUrl ? x.Name : x.GroupName,
                Username = nameIsUrl ? string.Empty : x.Name,
                Password = x.Value
            };
        });
};
