using System.Text.Json.Serialization;
using VaultKeeper.Models.ApplicationData;

namespace VaultKeeper.Services.DataFormatting;

[JsonSerializable(typeof(SavedData<AppConfigData>))]
[JsonSerializable(typeof(SavedData<UserData>))]
[JsonSerializable(typeof(SavedData<EntityData>))]
[JsonSerializable(typeof(SavedData<BackupData>))]
internal partial class VaultKeeperJsonSerializerContext : JsonSerializerContext;