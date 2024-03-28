using BepInEx;
using BepInEx.Logging;

using HarmonyLib;
using System;

namespace CSync;

/// <summary>
/// The plugin and main entry point of this library, responsible for holding the logger.<br></br>
/// Helps with internal logging and caching of config files through static references.<br></br>
/// <br></br>
/// Do <b>NOT</b> use this class directly or create any instances of it!
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal new static ManualLogSource Logger { get; private set; } = null!;

    Harmony Patcher;

    private void Awake() {
        Logger = base.Logger;

        try {
            Patcher = new(MyPluginInfo.PLUGIN_GUID);
            Patcher.PatchAll();

            Logger.LogInfo("CSync successfully applied patches.");
        } catch(Exception e) {
            Logger.LogError(e);
        }
    }
}
