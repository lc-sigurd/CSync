using System;
using BepInEx.Configuration;
using BepInEx;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CSync.Extensions;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

namespace CSync.Lib;

/// <summary>
/// Helper class enabling the user to easily setup CSync.<br></br>
/// Handles config registration, instance syncing and caching of BepInEx files.<br></br>
/// </summary>
[PublicAPI]
public static class ConfigManager {
    internal static readonly Dictionary<string, ConfigFile> FileCache = [];
    internal static readonly Dictionary<InstanceKey, ISyncedConfig> Instances = [];
    internal static readonly Dictionary<InstanceKey, EventHandler> InitialSyncHandlers = [];

    private static event Action? OnPopulateEntriesRequested;
    internal static void PopulateEntries() => OnPopulateEntriesRequested?.Invoke();

    private static readonly Lazy<GameObject> LazyPrefab;
    internal static GameObject Prefab => LazyPrefab.Value;

    static ConfigManager()
    {
        LazyPrefab = new Lazy<GameObject>(() =>
        {
            var container = new GameObject("CSyncPrefabContainer")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            container.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(container);

            var prefab = new GameObject("ConfigSyncHolder");
            prefab.transform.SetParent(container.transform);
            var networkObject = prefab.AddComponent<NetworkObject>();
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes($"{MyPluginInfo.PLUGIN_GUID}:ConfigSyncHolder"));
            networkObject.GlobalObjectIdHash = BitConverter.ToUInt32(hash);

            return prefab;
        });
    }

    internal static void AddToFileCache(ConfigFile configFile)
    {
        FileCache.TryAdd(configFile.GetConfigFileRelativePath(), configFile);
    }

    internal static ConfigFile GetConfigFile(string relativePath)
    {
        if (FileCache.TryGetValue(relativePath, out ConfigFile configFile))
            return configFile;

        string absolutePath = Path.GetFullPath(Path.Combine(Paths.BepInExRootPath, relativePath));
        configFile = new(absolutePath, false);
        FileCache.Add(relativePath, configFile);
        return configFile;
    }

    /// <summary>
    /// Register a config with CSync, making it responsible for synchronization.<br></br>
    /// After calling this method, all clients will receive the host's config upon joining.
    /// </summary>
    public static void Register<T>(T config) where T : SyncedConfig<T> {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config), "Config instance is null, cannot register.");
        }

        var assemblyQualifiedTypeName = typeof(T).AssemblyQualifiedName ?? throw new ArgumentException(nameof(config));
        var key = new InstanceKey(config.GUID, assemblyQualifiedTypeName);

        try {
            Instances.Add(key, config);
            InitialSyncHandlers.Add(key, config.OnInitialSyncCompleted);
        }
        catch (ArgumentException exc) {
            throw new InvalidOperationException($"Attempted to register config instance of type `{typeof(T)}`, but an instance has already been registered.", exc);
        }

        Plugin.Logger.LogDebug($"Successfully registered config instance {key}.");

        SyncedInstance<T>.Instance = config;
        SyncedInstance<T>.Default = config;
        OnPopulateEntriesRequested += config.PopulateEntryContainer;

        var syncBehaviour = Prefab.AddComponent<ConfigSyncBehaviour>();
        syncBehaviour.ConfigInstanceKey = key;
    }

    [UsedImplicitly]
    [Serializable]
    public record struct InstanceKey
    {
        public InstanceKey(string guid, string assemblyQualifiedName)
        {
            _guid = guid;
            _assemblyQualifiedName = assemblyQualifiedName;
        }

        [SerializeField]
        private string _guid;
        [SerializeField]
        private string _assemblyQualifiedName;

        public string Guid => _guid;
        public string AssemblyQualifiedName => _assemblyQualifiedName;
    }
}
