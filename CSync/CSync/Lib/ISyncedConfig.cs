namespace CSync.Lib;

public interface ISyncedConfig
{
    public string GUID { get; }

    public ISyncedEntryContainer EntryContainer { get; }
}
