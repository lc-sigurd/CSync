using System.Collections.Generic;

namespace CSync.Lib;

public interface ISyncedEntryContainer : IDictionary<(string ConfigFileRelativePath, SyncedConfigDefinition Definition), SyncedEntryBase>;
