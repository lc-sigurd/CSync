using System;
using BepInEx.Configuration;

namespace CSync.Lib;

/// <summary>
/// Wrapper class around a BepInEx <see cref="ConfigEntry{T}"/>.<br>
/// Can serialize and deserialize itself to avoid runtime errors when syncing configs.</br>
/// </summary>
public sealed class SyncedEntry<T> : SyncedEntryBase
{
    public ConfigEntry<T> Entry { get; private set; }

    public override ConfigEntryBase BoxedEntry
    {
        get => Entry;
        protected init => Entry = (ConfigEntry<T>) value;
    }

    public T LocalValue
    {
        get => Entry.Value;
        set => Entry.Value = value!;
    }

    private T _typedValueOverride;

    public override object? BoxedValueOverride
    {
        get => _typedValueOverride;
        set => _typedValueOverride = (T) value!;
    }

    public T Value {
        get {
            if (ValueOverridden) return _typedValueOverride!;
            return LocalValue;
        }
    }

    public static implicit operator T(SyncedEntry<T> e) => e.Value;

    public event EventHandler SettingChanged {
        add => Entry.SettingChanged += value;
        remove => Entry.SettingChanged -= value;
    }

    public SyncedEntry(ConfigEntry<T> entry) : base(entry) { }

    public override string ToString() {
        return $"Key: {Entry.Definition.Key}\nLocal Value: {LocalValue}\nCurrent Value: {Value}";
    }
}
