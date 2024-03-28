using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CSync.Lib;

public class SyncedEntryContainer : Dictionary<(string ConfigFileRelativePath, SyncedConfigDefinition Definition), SyncedEntryBase>, ISyncedEntryContainer
{
    public bool TryGetEntry<T>(string configFileRelativePath, SyncedConfigDefinition configDefinition, [MaybeNullWhen(false)] out SyncedEntry<T> entry)
    {
        if (TryGetValue((configFileRelativePath, configDefinition), out var entryBase))
        {
            entry = (SyncedEntry<T>)entryBase;
            return true;
        }

        entry = null;
        return false;
    }

    public bool TryGetEntry<T>(string configFileRelativePath, string section, string key, [MaybeNullWhen(false)] out SyncedEntry<T> entry)
    {
        return TryGetEntry<T>(configFileRelativePath, new SyncedConfigDefinition(section, key), out entry);
    }
}
