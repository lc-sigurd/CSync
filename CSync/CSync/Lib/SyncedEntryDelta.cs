using System;
using Unity.Collections;
using Unity.Netcode;

namespace CSync.Lib;

internal struct SyncedEntryDelta : INetworkSerializable, IEquatable<SyncedEntryDelta>
{
    public SyncedConfigDefinition Definition;
    public FixedString128Bytes ConfigFileRelativePath;
    public FixedString512Bytes SerializedValue;
    public bool SyncEnabled;

    public SyncedEntryDelta(
        FixedString128Bytes configFileRelativePath,
        SyncedConfigDefinition definition,
        FixedString512Bytes serializedValue,
        bool syncEnabled
    ) {
        ConfigFileRelativePath = configFileRelativePath;
        Definition = definition;
        SerializedValue = serializedValue;
        SyncEnabled = syncEnabled;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref Definition);

            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out ConfigFileRelativePath);
            reader.ReadValueSafe(out SerializedValue);
            reader.ReadValueSafe(out SyncEnabled);
        }
        else
        {
            serializer.SerializeValue(ref Definition);

            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(ConfigFileRelativePath);
            writer.WriteValueSafe(SerializedValue);
            writer.WriteValue(SyncEnabled);
        }
    }

    public bool Equals(SyncedEntryDelta other)
    {
        if (!Definition.Equals(other.Definition)) return false;
        if (!ConfigFileRelativePath.Equals(other.ConfigFileRelativePath)) return false;
        if (!SerializedValue.Equals(other.SerializedValue)) return false;
        if (!SyncEnabled.Equals(other.SyncEnabled)) return false;
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is SyncedEntryDelta other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Definition, ConfigFileRelativePath, SerializedValue, SyncEnabled);
    }

    public (string ConfigFileRelativePath, SyncedConfigDefinition Definition) SyncedEntryIdentifier
        => (ConfigFileRelativePath.Value, Definition);
}
