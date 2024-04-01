using System.IO;
using BepInEx.Configuration;
using CSync.Extensions;

namespace CSync.Lib;

public abstract class SyncedEntryBase
{
    public abstract ConfigEntryBase BoxedEntry { get; protected init; }

    public abstract object? BoxedValueOverride { get; set; }
    public virtual bool ValueOverridden { get; internal set; } = false;

    internal SyncedEntryBase(ConfigEntryBase configEntry)
    {
        BoxedEntry = configEntry;
        BoxedValueOverride = configEntry.DefaultValue;
    }

    public void SetSerializedValueOverride(string value)
    {
        BoxedValueOverride = TomlTypeConverter.ConvertToValue(value, BoxedEntry.SettingType);
    }

    internal SyncedEntryDelta ToDelta() => new SyncedEntryDelta(
        configFileRelativePath: BoxedEntry.ConfigFile.GetConfigFileRelativePath(),
        definition: BoxedEntry.Definition.ToSynced(),
        serializedValue: BoxedEntry.GetSerializedValue()
    );
}
