using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using CSync.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace CSync.Lib;

/// <summary>
/// Wrapper class allowing the config class (type parameter) to be synchronized.<br></br>
/// Stores the mod's unique identifier and handles registering and sending of named messages.
/// </summary>
[PublicAPI]
public class SyncedConfig2<T> : ISyncedConfig where T : SyncedConfig2<T>
{
    public ISyncedEntryContainer EntryContainer { get; } = new SyncedEntryContainer();
    private int _entryContainerPopulated = 0;

    public SyncedConfig2(string guid)
    {
        GUID = guid;
    }

    /// <summary>
    /// The mod name or abbreviation. After being given to the constructor, it cannot be changed.
    /// </summary>
    public string GUID { get; }

    private static Lazy<FieldInfo[]> SyncedEntryFields = new(
        () => AccessTools.GetDeclaredFields(typeof(T))
            .Where(field => field.GetCustomAttribute<SyncedEntryFieldAttribute>() is not null)
            .Where(field => typeof(SyncedEntryBase).IsAssignableFrom(field.FieldType))
            .ToArray()
    );

    internal void PopulateEntryContainer()
    {
        if (Interlocked.Exchange(ref _entryContainerPopulated, 1) != 0) return;
        foreach (var fieldInfo in SyncedEntryFields.Value)
        {
            var entryBase = (SyncedEntryBase)fieldInfo.GetValue(this);
            EntryContainer.Add(entryBase.BoxedEntry.ToSyncedEntryIdentifier(), entryBase);
        }
    }

    public event EventHandler? InitialSyncCompleted;
    internal void OnInitialSyncCompleted(object sender, EventArgs e) => InitialSyncCompleted?.Invoke(sender, e);
}
