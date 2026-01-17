using VaultKeeper.Models.ApplicationData.Files;

namespace VaultKeeper.Services.Abstractions;

public interface IAppFileDefinitionService
{
    AppFileDefinition GetFileDefinitionByType(AppFileType fileType);
}