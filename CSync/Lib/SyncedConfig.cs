using System;
using Unity.Collections;
using Unity.Netcode;

using CSync.Util;

namespace CSync.Lib;

/// <summary>
/// Wrapper class allowing the config class (type parameter) to be synchronized.<br></br>
/// Stores the mod's unique identifier and handles registering and sending of named messages.
/// </summary>
[Serializable]
public class SyncedConfig<T>(string guid) : SyncedInstance<T>, ISynchronizable where T : class {
    static void LogErr(string str) => Plugin.Logger.LogError(str);
    static void LogDebug(string str) => Plugin.Logger.LogDebug(str);

    /// <summary>
    /// Invoked on the host when a client requests to sync.
    /// </summary>
    [field:NonSerialized] public event EventHandler SyncRequested;
    internal void OnSyncRequested() => SyncRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Invoked on the client when they receive the host config.
    /// </summary>
    [field:NonSerialized] public event EventHandler SyncReceived;
    internal void OnSyncReceived() => SyncReceived?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// The mod name or abbreviation. After being given to the constructor, it cannot be changed.
    /// </summary>
    public readonly string GUID = guid;

    internal SyncedEntry<bool> SYNC_TO_CLIENTS { get; private set; } = null;

    /// <summary>
    /// Allow the host to control whether clients can use their own config.
    /// This MUST be called after binding the entry parameter.
    /// </summary>
    /// <param name="hostSyncControlOption">The entry for the host to use in your config file.</param>
    protected void EnableHostSyncControl(SyncedEntry<bool> hostSyncControlOption) {
        SYNC_TO_CLIENTS = hostSyncControlOption;

        hostSyncControlOption.SettingChanged += (object sender, EventArgs e) => {
            SYNC_TO_CLIENTS = hostSyncControlOption;
        };
    }

    void ISynchronizable.SetupSync() {
        if (IsHost) {
            MessageManager.RegisterNamedMessageHandler($"{GUID}_OnRequestConfigSync", OnRequestSync);
            return;
        }

        MessageManager.RegisterNamedMessageHandler($"{GUID}_OnHostDisabledSyncing", OnHostDisabledSyncing);
        MessageManager.RegisterNamedMessageHandler($"{GUID}_OnReceiveConfigSync", OnReceiveSync);
        RequestSync();
    }

    void RequestSync() {
        if (!IsClient) return;

        using FastBufferWriter stream = new(IntSize, Allocator.Temp);

        // Method `OnRequestSync` will then get called on the host.
        stream.SendMessage(GUID, "OnRequestConfigSync");
    }

    internal void OnRequestSync(ulong clientId, FastBufferReader _) {
        if (!IsHost) return;
        OnSyncRequested();

        if (SYNC_TO_CLIENTS != null && SYNC_TO_CLIENTS == false) {
            using FastBufferWriter s = new(IntSize, Allocator.Temp);
            s.SendMessage(GUID, "OnHostDisabledSyncing", clientId);

            LogDebug($"{GUID} - The host (you) has disabled syncing, sending clients a message!");
            return;
        }

        LogDebug($"{GUID} - Config sync request received from client: {clientId}");

        byte[] array = SerializeToBytes(Instance);
        int value = array.Length;

        using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

        try {
            stream.WriteValueSafe(in value, default);
            stream.WriteBytesSafe(array);

            stream.SendMessage(GUID, "OnReceiveConfigSync", clientId);
        } catch(Exception e) {
            LogErr($"{GUID} - Error occurred syncing config with client: {clientId}\n{e}");
        }
    }

    internal void OnReceiveSync(ulong _, FastBufferReader reader) {
        OnSyncReceived();

        if (!reader.TryBeginRead(IntSize)) {
            LogErr($"{GUID} - Config sync error: Could not begin reading buffer.");
            return;
        }

        reader.ReadValueSafe(out int val, default);
        if (!reader.TryBeginRead(val)) {
            LogErr($"{GUID} - Config sync error: Host could not sync.");
            return;
        }

        byte[] data = new byte[val];
        reader.ReadBytesSafe(ref data, val);

        try {
            SyncInstance(data);
        } catch(Exception e) {
            LogErr($"Error syncing config instance!\n{e}");
        }
    }

    internal void OnHostDisabledSyncing(ulong _, FastBufferReader reader) {
        OnSyncCompleted();
        LogDebug($"{GUID} - Host disabled syncing. The SyncComplete event will still be invoked.");
    }
}
