using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using CSync.Util;
using HarmonyLib;
using JetBrains.Annotations;

namespace CSync.Lib;

/// <summary>
/// Wrapper class allowing the config class (type parameter) to be synchronized.<br></br>
/// Stores the mod's unique identifier and handles registering and sending of named messages.
/// </summary>
public class SyncedConfig<T> : SyncedInstance<T>, ISyncedConfig where T : SyncedConfig<T>
{
    public ISyncedEntryContainer EntryContainer { get; } = new SyncedEntryContainer();

    static SyncedConfig()
    {
        var constructors = AccessTools.GetDeclaredConstructors(typeof(T));

        ConstructorInfo constructor;
        try
        {
            constructor = constructors.Single();
        }
        catch (InvalidOperationException exc)
        {
            throw new InvalidOperationException($"{typeof(T).Name} declares {constructors.Count} constructors. SyncedConfig subclasses must declare exactly one constructor.", exc);
        }

        Plugin.Patcher.Patch(constructor, postfix: new HarmonyMethod(AccessTools.Method(typeof(SyncedConfig<T>), nameof(PostConstructor))));
    }

    [HarmonyPostfix]
    [UsedImplicitly]
    static void PostConstructor(T __instance)
    {
        __instance.PopulateEntryContainer();
    }

    public SyncedConfig(string guid)
    {
        GUID = guid;
    }

    /// <summary>
    /// The mod name or abbreviation. After being given to the constructor, it cannot be changed.
    /// </summary>
    public string GUID { get; }

    private void PopulateEntryContainer()
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
}
