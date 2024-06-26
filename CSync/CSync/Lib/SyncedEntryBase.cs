using System;
using BepInEx.Configuration;
using CSync.Extensions;

namespace CSync.Lib;

public abstract class SyncedEntryBase
{
    public abstract ConfigEntryBase BoxedEntry { get; protected init; }

    public abstract object? BoxedValueOverride { get; set; }

    private bool _syncEnabled = true;
    public bool SyncEnabled {
        get => _syncEnabled;
        set {
            if (value == _syncEnabled) return;
            _syncEnabled = value;
            SyncEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public virtual bool ValueOverridden { get; internal set; } = false;

    internal SyncedEntryBase(ConfigEntryBase configEntry)
    {
        BoxedEntry = configEntry ?? throw new ArgumentNullException(nameof(configEntry));
        BoxedValueOverride = configEntry.DefaultValue;
    }

    internal event EventHandler? SyncEnabledChanged;

    public void SetSerializedValueOverride(string value)
    {
        BoxedValueOverride = TomlTypeConverter.ConvertToValue(value, BoxedEntry.SettingType);
    }

    internal SyncedEntryDelta ToDelta() => new SyncedEntryDelta(
        configFileRelativePath: BoxedEntry.ConfigFile.GetConfigFileRelativePath(),
        definition: BoxedEntry.Definition.ToSynced(),
        serializedValue: BoxedEntry.GetSerializedValue(),
        syncEnabled: SyncEnabled
    );
}
