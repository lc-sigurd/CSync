using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Netcode;
using UnityEngine;
using LogLevel = BepInEx.Logging.LogLevel;

namespace CSync.Lib;

public class ConfigSyncBehaviour : NetworkBehaviour
{
    [field: SerializeField]
    public ConfigManager.InstanceKey ConfigInstanceKey { get; internal set; }

    private ISyncedConfig? Config {
        get {
            var success = ConfigManager.Instances.TryGetValue(ConfigInstanceKey, out var config);
            return success ? config : null;
        }
    }

    private EventHandler? InitialSyncCompletedHandler {
        get {
            var success = ConfigManager.InitialSyncHandlers.TryGetValue(ConfigInstanceKey, out var handler);
            return success ? handler : null;
        }
    }

    private ISyncedEntryContainer? _entryContainer;
    internal ISyncedEntryContainer? EntryContainer => _entryContainer ??= Config?.EntryContainer;

    public bool SyncEnabled
    {
        get => _syncEnabled.Value;
        set => _syncEnabled.Value = value;
    }

    private readonly NetworkVariable<bool> _syncEnabled = new();
    private NetworkList<SyncedEntryDelta> _deltas = null!;
    private readonly List<SyncedEntryListener> _syncedEntryListeners = new();

    [MemberNotNull(nameof(EntryContainer))]
    private void EnsureEntryContainer()
    {
        if (EntryContainer is not null) return;
        throw new InvalidOperationException("Entry container has not been assigned.");
    }

    private void Awake()
    {
        EnsureEntryContainer();
        _deltas = new NetworkList<SyncedEntryDelta>();
    }

    // Helper class to maintain event subscriptions.
    // SyncedEntryDelta is a struct which can't be copied around, so it has to be stored by list+index.
    sealed class SyncedEntryListener : IDisposable
    {
        private readonly NetworkList<SyncedEntryDelta> deltas;
        private readonly int index;
        private readonly SyncedEntryBase syncedEntryBase;

        public SyncedEntryListener(NetworkList<SyncedEntryDelta> deltas, int index, SyncedEntryBase syncedEntryBase)
        {
            this.deltas = deltas;
            this.index = index;
            this.syncedEntryBase = syncedEntryBase;

            syncedEntryBase.BoxedEntry.ConfigFile.SettingChanged += OnSettingChanged;
            syncedEntryBase.SyncEnabledChanged += OnEntrySyncEnabledChanged;
        }

        public void Dispose()
        {
            syncedEntryBase.BoxedEntry.ConfigFile.SettingChanged -= OnSettingChanged;
            syncedEntryBase.SyncEnabledChanged -= OnEntrySyncEnabledChanged;
        }

        void OnSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (!ReferenceEquals(syncedEntryBase.BoxedEntry, args.ChangedSetting)) return;
            deltas[index] = syncedEntryBase.ToDelta();
        }

        void OnEntrySyncEnabledChanged(object sender, EventArgs args)
        {
            deltas[index] = syncedEntryBase.ToDelta();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        EnsureEntryContainer();

        if (IsServer)
        {
            _syncEnabled.Value = true;

            foreach (var syncedEntryBase in EntryContainer.Values)
            {
                var currentIndex = _deltas.Count;
                _deltas.Add(syncedEntryBase.ToDelta());
                _syncedEntryListeners.Add(new(_deltas, currentIndex, syncedEntryBase));
            }

            InitialSyncCompletedHandler?.Invoke(this, EventArgs.Empty);
        }
        else if (IsClient)
        {
            _syncEnabled.OnValueChanged += OnSyncEnabledChanged;
            _deltas.OnListChanged += OnClientDeltaListChanged;

            foreach (var delta in _deltas)
            {
                UpdateOverrideValue(delta);
            }

            if (_syncEnabled.Value) EnableOverrides();

            InitialSyncCompletedHandler?.Invoke(this, EventArgs.Empty);
        }
    }

    public override void OnNetworkDespawn()
    {
        EnsureEntryContainer();

        if (IsServer)
        {
            foreach (var listener in _syncedEntryListeners)
            {
                listener.Dispose();
            }

            _syncedEntryListeners.Clear();

            // NetworkList and NetworkVariable can not be modified now because the network manager has already shut down.
            // See https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/pull/3502
            // The fix applies to NGO 1.14.0+, but Lethal Company v73 is on NGO 1.12.0
            // Should not be needed anyway, since this component isn't being reused between network sessions.
            // _deltas.Clear();
            // _syncEnabled.Value = false;
        }
        else if (IsClient)
        {
            DisableOverrides();

            foreach (var delta in _deltas)
            {
                ResetOverrideValue(delta);
            }

            _deltas.OnListChanged -= OnClientDeltaListChanged;
            _syncEnabled.OnValueChanged -= OnSyncEnabledChanged;
        }

        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        // The content of this method has moved to OnNetworkDespawn.
        // Keep this method around for ABI compatibility though.
        base.OnDestroy();
    }

    private void OnSyncEnabledChanged(bool previousValue, bool newValue)
    {
        if (previousValue == newValue) return;

        if (newValue)
        {
            EnableOverrides();
        }
        else
        {
            DisableOverrides();
        }
    }

    private void OnClientDeltaListChanged(NetworkListEvent<SyncedEntryDelta> args)
    {
        switch (args.Type)
        {
            case NetworkListEvent<SyncedEntryDelta>.EventType.Remove:
            case NetworkListEvent<SyncedEntryDelta>.EventType.RemoveAt:
                ResetOverrideValue(args.PreviousValue);
                break;
            case NetworkListEvent<SyncedEntryDelta>.EventType.Add:
            case NetworkListEvent<SyncedEntryDelta>.EventType.Insert:
            case NetworkListEvent<SyncedEntryDelta>.EventType.Value:
                UpdateOverrideValue(args.Value);
                break;
            case NetworkListEvent<SyncedEntryDelta>.EventType.Clear:
                foreach (var delta in _deltas)
                {
                    ResetOverrideValue(delta);
                }
                break;
            case NetworkListEvent<SyncedEntryDelta>.EventType.Full:
                foreach (var delta in _deltas)
                {
                    UpdateOverrideValue(delta);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetOverrideValue(SyncedEntryDelta delta)
    {
        EnsureEntryContainer();
        try {
            var entry = EntryContainer[delta.SyncedEntryIdentifier];
            entry.BoxedValueOverride = entry.BoxedEntry.DefaultValue;
        }
        catch (KeyNotFoundException) { }
    }

    private void UpdateOverrideValue(SyncedEntryDelta delta)
    {
        EnsureEntryContainer();
        try {
            var entry = EntryContainer[delta.SyncedEntryIdentifier];
            entry.SetSerializedValueOverride(delta.SerializedValue.Value);
            entry.ValueOverridden = delta.SyncEnabled && SyncEnabled;
        }
        catch (KeyNotFoundException) {
            Plugin.Logger.Log(LogLevel.Warning, $"Setting \"{delta.Definition}\" could not be found, so its synced value override will be ignored.");
        }
        catch (Exception exc) {
            Plugin.Logger.Log(LogLevel.Warning, $"Synced value override of setting \"{delta.Definition}\" could not be parsed and will be ignored. Reason: {exc.Message}; Value: {delta.SerializedValue.Value}");
        }
    }

    private void EnableOverrides()
    {
        EnsureEntryContainer();
        foreach (var delta in _deltas)
        {
            try {
                var entry = EntryContainer[delta.SyncedEntryIdentifier];
                entry.ValueOverridden = delta.SyncEnabled;
            }
            catch (KeyNotFoundException) {
                Plugin.Logger.Log(LogLevel.Warning, $"Setting \"{delta.Definition}\" could not be found, so its value override could not be enabled.");
            }
        }
    }

    private void DisableOverrides()
    {
        EnsureEntryContainer();
        foreach (var syncedEntryBase in EntryContainer.Values)
        {
            syncedEntryBase.ValueOverridden = false;
        }
    }
}
