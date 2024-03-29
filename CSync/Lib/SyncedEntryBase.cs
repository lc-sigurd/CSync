using System.IO;
using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Util;

namespace CSync.Lib;

public abstract class SyncedEntryBase
{
    public abstract ConfigEntryBase BoxedEntry { get; protected init; }

    public abstract object? BoxedValueOverride { get; set; }
    protected internal bool ValueOverridden = false;

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
        configFileRelativePath: Path.GetFileName(BoxedEntry.ConfigFile.ConfigFilePath),
        definition: BoxedEntry.Definition.ToSynced(),
        serializedValue: BoxedEntry.GetSerializedValue()
    );
}
