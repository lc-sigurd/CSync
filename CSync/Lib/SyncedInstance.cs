using CSync.Util;

namespace CSync.Lib;

public class SyncedInstance<T> : ByteSerializer<T> where T : class
{
    public static T? Instance { get; internal set; }
}
