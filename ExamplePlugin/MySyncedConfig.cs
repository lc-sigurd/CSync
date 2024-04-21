using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;

namespace CSync.ExamplePlugin;

public class MySyncedConfig : SyncedConfig2<MySyncedConfig>
{
    [field: SyncedEntryField]
    public SyncedEntry<float> ClimbSpeed { get; }

    public MySyncedConfig(ConfigFile configFile) : base(MyPluginInfo.PLUGIN_GUID)
    {
        ClimbSpeed = ClimbSpeed = configFile.BindSyncedEntry(
            new ConfigDefinition("Movement", "Climb Speed"),
            3.9f,
            new ConfigDescription("The base speed at which the player climbs.")
        );

        ConfigManager.Register(this);
    }
}
