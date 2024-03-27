using System;
using BepInEx.Configuration;
using Unity.Collections;
using Unity.Netcode;

namespace CSync.Lib;

internal struct SyncedConfigDefinition : INetworkSerializable, System.IEquatable<SyncedConfigDefinition>
{
    public FixedString128Bytes Section;
    public FixedString128Bytes Key;

    public SyncedConfigDefinition(FixedString128Bytes section, FixedString128Bytes key)
    {
        Section = section;
        Key = key;
    }

    public readonly ConfigDefinition ToConfigDefinition()
    {
        return new ConfigDefinition(Section.Value, Key.Value);
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
