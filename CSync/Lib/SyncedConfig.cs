using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CSync.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace CSync.Lib;

/// <summary>
/// Wrapper class allowing the config class (type parameter) to be synchronized.<br></br>
/// Stores the mod's unique identifier and handles registering and sending of named messages.
/// </summary>
[PublicAPI]
public class SyncedConfig<T> : SyncedInstance<T>, ISyncedConfig where T : SyncedConfig<T>
{
    public ISyncedEntryContainer EntryContainer { get; } = new SyncedEntryContainer();

    public SyncedConfig(string guid)
    {
        GUID = guid;
    }

    /// <summary>
    /// The mod name or abbreviation. After being given to the constructor, it cannot be changed.
    /// </summary>
    public string GUID { get; }

    internal void PopulateEntryContainer()
    {
        var fields = AccessTools.GetDeclaredFields(typeof(T))
            .Where(field => field.GetCustomAttribute<DataMemberAttribute>() is not null)
            .Where(field => typeof(SyncedEntryBase).IsAssignableFrom(field.FieldType));

        foreach (var fieldInfo in fields)
        {
            var entryBase = (SyncedEntryBase)fieldInfo.GetValue(this);
            EntryContainer.Add(entryBase.BoxedEntry.ToSyncedEntryIdentifier(), entryBase);
        }
    }

    public event EventHandler? InitialSyncCompleted;
    internal void OnInitialSyncCompleted(object sender, EventArgs e) => InitialSyncCompleted?.Invoke(sender, e);
}
