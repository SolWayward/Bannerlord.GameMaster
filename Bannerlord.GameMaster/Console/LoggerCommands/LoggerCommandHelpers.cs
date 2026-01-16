namespace Bannerlord.GameMaster.Console.LoggerCommands;

/// <summary>
/// Helper methods for logger commands
/// </summary>
public static class LoggerCommandHelpers
{
    /// <summary>
    /// Format file size in human-readable format
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 bytes";
        if (bytes < 1024) return $"{bytes} bytes";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
