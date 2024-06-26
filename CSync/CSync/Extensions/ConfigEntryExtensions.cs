using BepInEx.Configuration;
using CSync.Lib;

namespace CSync.Extensions;

internal static class ConfigEntryExtensions
{
    public static (string ConfigFileRelativePath, SyncedConfigDefinition Definition) ToSyncedEntryIdentifier(
        this ConfigEntryBase entry)
    {
        return (entry.ConfigFile.GetConfigFileRelativePath(), entry.Definition.ToSynced());
    }
}
