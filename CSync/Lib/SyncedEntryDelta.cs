using System;
using Unity.Collections;
using Unity.Netcode;

namespace CSync.Lib;

internal struct SyncedEntryDelta : INetworkSerializable, IEquatable<SyncedEntryDelta>
{
    public SyncedConfigDefinition Definition;
    public FixedString128Bytes ConfigFileRelativePath;
    public FixedString512Bytes SerializedValue;

    public SyncedEntryDelta(FixedString128Bytes configFileRelativePath, SyncedConfigDefinition definition, FixedString512Bytes serializedValue)
    {
        ConfigFileRelativePath = configFileRelativePath;
        Definition = definition;
        SerializedValue = serializedValue;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref Definition);

            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out ConfigFileRelativePath);
            reader.ReadValueSafe(out SerializedValue);
        }
        else
        {
            serializer.SerializeValue(ref Definition);

            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(ConfigFileRelativePath);
            writer.WriteValueSafe(SerializedValue);
        }
    }

    public bool Equals(SyncedEntryDelta other)
    {
        return Definition.Equals(other.Definition) && ConfigFileRelativePath.Equals(other.ConfigFileRelativePath) && SerializedValue.Equals(other.SerializedValue);
    }

    public override bool Equals(object? obj)
    {
        return obj is SyncedEntryDelta other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Definition, ConfigFileRelativePath, SerializedValue);
    }
}
