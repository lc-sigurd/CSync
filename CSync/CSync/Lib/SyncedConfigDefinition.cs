using System;
using Unity.Collections;
using Unity.Netcode;

namespace CSync.Lib;

public struct SyncedConfigDefinition : INetworkSerializable, IEquatable<SyncedConfigDefinition>
{
    public FixedString128Bytes Section;
    public FixedString128Bytes Key;

    public SyncedConfigDefinition(FixedString128Bytes section, FixedString128Bytes key)
    {
        Section = section;
        Key = key;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out Section);
            reader.ReadValueSafe(out Key);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(Section);
            writer.WriteValueSafe(Key);
        }
    }

    public bool Equals(SyncedConfigDefinition other)
    {
        return Section.Equals(other.Section) && Key.Equals(other.Key);
    }

    public override bool Equals(object? obj)
    {
        return obj is SyncedConfigDefinition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Section, Key);
    }
}
