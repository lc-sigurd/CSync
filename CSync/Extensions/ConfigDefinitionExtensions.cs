using BepInEx.Configuration;
using CSync.Lib;

namespace CSync.Extensions;

internal static class ConfigDefinitionExtensions
{
    public static SyncedConfigDefinition ToSynced(this ConfigDefinition definition)
    {
        return new(definition.Section, definition.Key);
    }
}
