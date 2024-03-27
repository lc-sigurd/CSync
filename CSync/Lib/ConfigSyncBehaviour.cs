using System;
using Unity.Netcode;

namespace CSync.Lib;

public class ConfigSyncBehaviour : NetworkBehaviour
{
    private NetworkVariable<bool> HostDisabledSync = new();
    private NetworkList<SyncedEntryDelta> Deltas;

    private void Awake()
    {
        Deltas = new NetworkList<SyncedEntryDelta>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            HostDisabledSync.Value = false; // todo: use config value here
            // todo: populate the list
        }

        if (IsClient)
        {
            Deltas.OnListChanged += OnClientDeltaListChanged;
        }
    }

    private void OnClientDeltaListChanged(NetworkListEvent<SyncedEntryDelta> args)
    {
        throw new NotImplementedException();
    }
}
