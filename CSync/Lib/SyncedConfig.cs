using System;

namespace CSync.Lib;

/// <summary>
/// Wrapper class allowing the config class (type parameter) to be synchronized.<br></br>
/// Stores the mod's unique identifier and handles registering and sending of named messages.
/// </summary>
public class SyncedConfig<T>(string guid) : ISynchronizable where T : class
{
    static void LogErr(string str) => Plugin.Logger.LogError(str);
    static void LogDebug(string str) => Plugin.Logger.LogDebug(str);

    /// <summary>
    /// Invoked on the host when a client requests to sync.
    /// </summary>
    public event EventHandler? SyncRequested;
    internal void OnSyncRequested() => SyncRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Invoked on the client when they receive the host config.
    /// </summary>
    public event EventHandler? SyncReceived;
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

    public void SetupSync()
    {
        throw new NotImplementedException();
    }

    public void RevertSync()
    {
        throw new NotImplementedException();
    }
}
