using BepInEx.Configuration;
using CSync.Lib;

namespace CSync.Extensions;

/// <summary>
/// Contains helpful extension methods to aid with synchronization and reduce code duplication.
/// </summary>
public static class SyncedBindingExtensions {
    /// <summary>
    /// Binds an entry to this file and returns the converted synced entry.
    /// </summary>
    /// <param name="configFile">The currently selected config file.</param>
    /// <param name="section">The category that this entry should show under.</param>
    /// <param name="key">The name/identifier of this entry.</param>
    /// <param name="defaultVal">The value assigned to this entry if not changed.</param>
    /// <param name="description">The description indicating what this entry does.</param>
    public static SyncedEntry<V> BindSyncedEntry<V>(
        this ConfigFile configFile,
        string section,
        string key,
        V defaultVal,
        string description
    ) {
        return configFile.BindSyncedEntry(new ConfigDefinition(section, key), defaultVal, new ConfigDescription(description));
    }

    public static SyncedEntry<V> BindSyncedEntry<V>(
        this ConfigFile configFile,
        string section,
        string key,
        V defaultValue,
        ConfigDescription? desc = null
    ) {
        return configFile.BindSyncedEntry(new ConfigDefinition(section, key), defaultValue, desc);
    }

    public static SyncedEntry<V> BindSyncedEntry<V>(
        this ConfigFile configFile,
        ConfigDefinition definition,
        V defaultValue,
        string description
    ) {
        return configFile.BindSyncedEntry(definition, defaultValue, new ConfigDescription(description));
    }

    public static SyncedEntry<V> BindSyncedEntry<V>(
        this ConfigFile configFile,
        ConfigDefinition definition,
        V defaultValue,
        ConfigDescription? description = null
    ) {
        ConfigManager.AddToFileCache(configFile);
        return configFile.Bind(definition, defaultValue, description).ToSyncedEntry();
    }

    /// <summary>
    /// Converts this entry into a serializable alternative, allowing it to be synced.
    /// </summary>
    public static SyncedEntry<V> ToSyncedEntry<V>(this ConfigEntry<V> entry) {
        return new SyncedEntry<V>(entry);
    }
}
