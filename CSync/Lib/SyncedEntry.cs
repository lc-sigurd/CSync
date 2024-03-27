using System;
using System.IO;
using BepInEx.Configuration;
using CSync.Util;

namespace CSync.Lib;

/// <summary>
/// Wrapper class around a BepInEx <see cref="ConfigEntry{T}"/>.<br>
/// Can serialize and deserialize itself to avoid runtime errors when syncing configs.</br>
/// </summary>
public class SyncedEntry<V>
{
    public ConfigEntry<V> Entry { get; }

    public V LocalValue
    {
        get => Entry.Value;
        set => Entry.Value = value!;
    }

    protected V _valueOverride;
    protected bool _valueOverridden;

    public V Value {
        get {
            if (_valueOverridden) return _valueOverride;
            return LocalValue;
        }
    }

    public static implicit operator V(SyncedEntry<V> e) => e.Value;

    public event EventHandler SettingChanged {
        add => Entry.SettingChanged += value;
        remove => Entry.SettingChanged -= value;
    }

    public SyncedEntry(ConfigEntry<V> entry)
    {
        Entry = entry;
    }

    public override string ToString() {
        return $"Key: {Entry.Definition.Key}\nLocal Value: {LocalValue}\nCurrent Value: {Value}";
    }

    internal SyncedEntryDelta ToDelta() => new SyncedEntryDelta(
        configFileName: Path.GetFileName(Entry.ConfigFile.ConfigFilePath),
        definition: Entry.Definition.ToSynced(),
        serializedValue: TomlTypeConverter.ConvertToString(LocalValue, typeof(V))
    );
}
