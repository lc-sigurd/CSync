using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace CSync.Util;

internal static class ConfigFileExtensions
{
    public static string GetConfigFileRelativePath(this ConfigFile configFile)
    {
        return Path.GetRelativePath(Paths.BepInExRootPath, configFile.ConfigFilePath);
    }
}
