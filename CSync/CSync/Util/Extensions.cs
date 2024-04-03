using System;
using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;

namespace CSync.Util;

/// <summary>
/// Contains helpful extension methods to aid with synchronization and reduce code duplication.
/// </summary>
[Obsolete($"Use {nameof(SyncedBindingExtensions)} instead.")]
public static class Extensions {
    /// <summary>
    /// Binds an entry to this file and returns the converted synced entry.
    /// </summary>
    /// <param name="configFile">The currently selected config file.</param>
    /// <param name="section">The category that this entry should show under.</param>
    /// <param name="key">The name/identifier of this entry.</param>
    /// <param name="defaultValue">The value assigned to this entry if not changed.</param>
    /// <param name="description">The description indicating what this entry does.</param>
    [Obsolete($"Use {nameof(SyncedBindingExtensions)} instead.")]
    public static SyncedEntry<T> BindSyncedEntry<T>(
        this ConfigFile configFile,
        string section,
        string key,
        T defaultValue,
        string description
    ) {
        return SyncedBindingExtensions.BindSyncedEntry<T>(
            configFile,
            section,
            key,
            defaultValue,
            description
        );
    }

    [Obsolete($"Use {nameof(SyncedBindingExtensions)} instead.")]
    public static SyncedEntry<T> BindSyncedEntry<T>(
        this ConfigFile configFile,
        string section,
        string key,
        T defaultValue,
        ConfigDescription? description = null
    ) {
        return SyncedBindingExtensions.BindSyncedEntry<T>(
            configFile,
            section,
            key,
            defaultValue,
            description
        );
    }

    [Obsolete($"Use {nameof(SyncedBindingExtensions)} instead.")]
    public static SyncedEntry<T> BindSyncedEntry<T>(
        this ConfigFile configFile,
        ConfigDefinition definition,
        T defaultValue,
        string description
    ) {
        return SyncedBindingExtensions.BindSyncedEntry<T>(
            configFile,
            definition,
            defaultValue,
            description
        );
    }

    [Obsolete($"Use {nameof(SyncedBindingExtensions)} instead.")]
    public static SyncedEntry<T> BindSyncedEntry<T>(
        this ConfigFile configFile,
        ConfigDefinition definition,
        T defaultValue,
        ConfigDescription? description = null
    ) {
        return SyncedBindingExtensions.BindSyncedEntry<T>(
            configFile,
            definition,
            defaultValue,
            description
        );
    }

    /// <summary>
    /// Converts this entry into a serializable alternative, allowing it to be synced.
    /// </summary>
    [Obsolete($"Use {nameof(SyncedBindingExtensions)} instead.")]
    public static SyncedEntry<T> ToSyncedEntry<T>(this ConfigEntry<T> entry)
    {
        return SyncedBindingExtensions.ToSyncedEntry(entry);
    }
}
