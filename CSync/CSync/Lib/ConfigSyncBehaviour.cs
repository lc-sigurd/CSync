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

    public override void OnNetworkSpawn()
    {
        EnsureEntryContainer();

        if (IsServer)
        {
            _syncEnabled.Value = true;

            foreach (var syncedEntryBase in EntryContainer.Values)
            {
                var currentIndex = _deltas.Count;
                _deltas.Add(syncedEntryBase.ToDelta());

                syncedEntryBase.BoxedEntry.ConfigFile.SettingChanged += (_, args) =>
                {
                    if (!ReferenceEquals(syncedEntryBase.BoxedEntry, args.ChangedSetting)) return;
                    _deltas[currentIndex] = syncedEntryBase.ToDelta();
                };

                syncedEntryBase.SyncEnabledChanged += (_, args) =>
                {
                    _deltas[currentIndex] = syncedEntryBase.ToDelta();
                };
            }

            InitialSyncCompletedHandler?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (IsClient)
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

    public override void OnDestroy()
    {
        DisableOverrides();
        foreach (var delta in _deltas)
        {
            ResetOverrideValue(delta);
        }
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
