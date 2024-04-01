using System;
using System.Diagnostics.CodeAnalysis;
using CSync.Lib;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CSync.Patches;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(StartOfRound))]
public static class StartOfRoundPatch
{
    [HarmonyPatch(nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    public static void OnSessionStart(StartOfRound __instance)
    {
        ConfigManager.PopulateEntries();

        if (!__instance.IsOwner) return;

        try {
            var configManagerGameObject = Object.Instantiate(ConfigManager.Prefab, __instance.transform);
            configManagerGameObject.hideFlags = HideFlags.None;
            configManagerGameObject.GetComponent<NetworkObject>().Spawn();
        }
        catch (Exception exc) {
            Plugin.Logger.LogError($"Failed to instantiate config sync behaviours:\n{exc}");
        }
    }
}
