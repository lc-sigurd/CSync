using CSync.Util;

namespace CSync.Lib;

public class SyncedInstance<T> : ByteSerializer<T>
{
    public T? Instance { get; internal set; }
}
