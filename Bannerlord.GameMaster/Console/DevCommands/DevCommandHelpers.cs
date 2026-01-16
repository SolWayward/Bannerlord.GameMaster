using System;
using System.IO;

namespace Bannerlord.GameMaster.Console.DevCommands;

/// <summary>
/// Helper methods for dev commands
/// </summary>
public static class DevCommandHelpers
{
    /// <summary>
    /// Gets the GameMaster config directory path in the Bannerlord documents folder.
    /// Creates the directory if it doesn't exist.
    /// </summary>
    /// <returns>The full path to the GameMaster config directory</returns>
    public static string GetOrCreateConfigDirectory()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string configDir = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs", "GameMaster");

        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        return configDir;
    }

    /// <summary>
    /// Gets the full path for a dump file in the GameMaster config directory
    /// </summary>
    /// <param name="fileName">The filename (with extension) for the dump file</param>
    /// <returns>The full path to the dump file</returns>
    public static string GetDumpFilePath(string fileName)
    {
        string configDir = GetOrCreateConfigDirectory();
        return Path.Combine(configDir, fileName);
    }
}
