using BepInEx.Configuration;
using CSync.Lib;

namespace CSync.Util;

internal static class ConfigEntryExtensions
{
    internal static SyncedConfigDefinition ToSynced(this ConfigDefinition definition)
    {
        return new(definition.Section, definition.Key);
    }
}
