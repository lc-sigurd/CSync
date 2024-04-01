using System.Diagnostics.CodeAnalysis;
using CSync.Lib;
using HarmonyLib;
using Unity.Netcode;

namespace CSync.Patches;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(GameNetworkManager))]
public static class GameNetworkManagerPatch
{
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    public static void OnNetworkManagerStart(GameNetworkManager __instance)
    {
        ConfigManager.PopulateEntries();

        if (NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(ConfigManager.Prefab))
            return;
        NetworkManager.Singleton.AddNetworkPrefab(ConfigManager.Prefab);
    }
}
