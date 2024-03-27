using System;
using Unity.Collections;
using Unity.Netcode;

namespace CSync.Lib;

internal struct SyncedEntryDelta : INetworkSerializable, IEquatable<SyncedEntryDelta>
{
    public SyncedConfigDefinition Definition;
    public FixedString128Bytes ConfigFileName;
    public FixedString512Bytes SerializedValue;

    public SyncedEntryDelta(FixedString128Bytes configFileName, SyncedConfigDefinition definition, FixedString512Bytes serializedValue)
    {
        ConfigFileName = configFileName;
        Definition = definition;
        SerializedValue = serializedValue;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref Definition);

            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out ConfigFileName);
            reader.ReadValueSafe(out SerializedValue);
        }
        else
        {
            serializer.SerializeValue(ref Definition);

            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(ConfigFileName);
            writer.WriteValueSafe(SerializedValue);
        }
    }

    public bool Equals(SyncedEntryDelta other)
    {
        return Definition.Equals(other.Definition) && ConfigFileName.Equals(other.ConfigFileName) && SerializedValue.Equals(other.SerializedValue);
    }

    public override bool Equals(object? obj)
    {
        return obj is SyncedEntryDelta other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Definition, ConfigFileName, SerializedValue);
    }
}
