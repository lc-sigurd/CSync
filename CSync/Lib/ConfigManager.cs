using System;
using BepInEx.Configuration;
using BepInEx;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CSync.Extensions;
using CSync.Util;
using Unity.Netcode;
using UnityEngine;

namespace CSync.Lib;

/// <summary>
/// Helper class enabling the user to easily setup CSync.<br></br>
/// Handles config registration, instance syncing and caching of BepInEx files.<br></br>
/// </summary>
public class ConfigManager {
    internal static readonly Dictionary<string, ConfigFile> FileCache = [];
    internal static readonly Dictionary<string, ISyncedConfig> Instances = [];

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

        string guid = config.GUID;

        try {
            Instances.Add(guid, config);
        }
        catch (ArgumentException exc) {
            throw new InvalidOperationException($"Attempted to register config `{guid}`, but it has already been registered.", exc);
        }

        var syncBehaviour = Prefab.AddComponent<ConfigSyncBehaviour>();
        syncBehaviour.ConfigGuid = config.GUID;
    }
}
