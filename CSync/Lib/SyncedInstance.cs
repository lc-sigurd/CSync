using System;
using CSync.Util;

namespace CSync.Lib;

public class SyncedInstance<T> : ByteSerializer<T> where T : class
{
    [Obsolete]
    public static T? Instance { get; internal set; }
    [Obsolete("This will never actually hold the local configured values. Use SyncedEntry.LocalValue instead.")]
    public static T? Default { get; internal set; }
}
