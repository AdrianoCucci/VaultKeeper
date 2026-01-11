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
    IAppDataService appDataService) : IImportService
{
    private static ImportSource[] Sources =>
    [
        new()
        {
            Type = ImportSourceType.Application,
            Name = "Vault Keeper",
            FileType = "*.csv",
            Icon = Icons.VaultKeeper,
            Description = "Import from a Vault Keeper CSV file."
        },
        new()
        {
            Type = ImportSourceType.Chromium,
            Name = "Chrome/Chromium",
            FileType = "*.csv",
            Icon = Icons.GoogleChrome,
            Description = "Import from a Google Chrome or other Chromium-based browser password CSV file.",
            AdditionalIcons = [Icons.MicrosoftEdge, Icons.BraveBrowser, Icons.OperaBrowser]
        },
        new()
        {
            Type = ImportSourceType.Firefox,
            Name = "Firefox",
            FileType = "*.csv",
            Icon = Icons.MozillaFirefox,
            Description = "Import from a Mozilla Firefox password CSV file."
        }
    ];

    public IEnumerable<ImportSource> GetImportSources()
    {
        logger.LogInformation(nameof(GetImportSources));
        return Sources;
    }

    public async Task<Result> ProcessImportAsync(ImportSourceType sourceType, string sourceFilePath)
    {
        logger.LogInformation($"{nameof(ProcessImportAsync)} | {nameof(sourceType)}: {{sourceType}}, {nameof(sourceFilePath)}: \"{{sourceFilePath}}\"", sourceType, sourceFilePath);

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

        VaultItemImportRecord[] duplicateImportNameRecords = [.. importRecords.DuplicatesBy(x => x.Name)];
        if (duplicateImportNameRecords.Length > 0)
            return Result.Failed(ResultFailureType.BadRequest, $"File contains rows with duplicate names: [{string.Join(", ", duplicateImportNameRecords.Select(x => $"\"{x.Name}\""))}]");

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

        NewVaultItem[] vaultItemsToCreate = [.. importRecords
            .Select(x => new NewVaultItem
            {
                Name  = x.Name,
                Value = x.Value,
                GroupId = existingGroups.FirstOrDefault(g => g.Name == x.GroupName)?.Id
            })];

        Result<IEnumerable<VaultItem>> createVaultItemsResult = await vaultItemService.AddManyAsync(vaultItemsToCreate, encrypt: true);
        if (!createVaultItemsResult.IsSuccessful)
            return createVaultItemsResult.Logged(logger);

        Result<SavedData<EntityData>?> saveDataResult = await appDataService.SaveEntityDataAsync();
        if (!saveDataResult.IsSuccessful)
            return saveDataResult.Logged(logger);

        return Result.Ok($"Imported {importRecords.Length} records successfully.").Logged(logger);
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

                    IEnumerable<VaultItemImportRecord> mappedRecords = deserializeResult.Value!.Select(x => new VaultItemImportRecord
                    {
                        Name = x.Username,
                        Value = x.Password,
                        GroupName = x.Url
                    });

                    result = mappedRecords.ToOkResult();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sourceType), $"Value is not a valid {nameof(ImportSourceType)} enum value.");
        }

        return result.Logged(logger);
    }
};
