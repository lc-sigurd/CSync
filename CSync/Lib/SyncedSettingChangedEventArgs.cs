using System;
using JetBrains.Annotations;

namespace CSync.Lib;

[PublicAPI]
public sealed class SyncedSettingChangedEventArgs<T> : EventArgs
{
    public required SyncedEntry<T> ChangedEntry { get; init; }
    public required T OldValue { get; init; }
    public required T NewValue { get; init; }
}
