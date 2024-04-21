using BepInEx;
using BepInEx.Logging;

namespace CSync.ExamplePlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.sigurd.csync", "5.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;

    internal new static MySyncedConfig Config { get; private set; }= null!;

    private void Awake()
    {
        Logger = base.Logger;
        Config = new MySyncedConfig(base.Config);
    }
}
