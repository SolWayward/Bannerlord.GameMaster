using System.Text;

namespace Bannerlord.GameMaster.Console.GeneralCommands;

/// <summary>
/// Helper methods for general commands
/// </summary>
public static class GeneralCommandHelpers
{
    /// <summary>
    /// Gets current status message including limits and counts
    /// </summary>
    public static string GetStatusMessage()
    {
        StringBuilder sb = new();
        sb.AppendLine("=== BLGM Object Creation Limits ===");
        sb.AppendLine();
        sb.AppendLine($"Ignore Limits: {BLGMObjectManager.IgnoreLimits}");
        sb.AppendLine();
        sb.Append(GetLimitsInfo());
        sb.AppendLine();
        sb.AppendLine("Usage: gm.ignore_limits <true|false|1|0>");
        sb.AppendLine("Note: Limits exist to prevent performance issues on the campaign map.");
        
        return sb.ToString();
    }

    /// <summary>
    /// Gets formatted limits and current counts
    /// </summary>
    public static string GetLimitsInfo()
    {
        StringBuilder sb = new();
        sb.AppendLine("Current Limits and Counts:");
        sb.AppendLine($"  Heroes:   {BLGMObjectManager.BlgmHeroCount,4} / {BLGMObjectManager.maxBlgmHeroes,4}");
        sb.AppendLine($"  Clans:    {BLGMObjectManager.BlgmClanCount,4} / {BLGMObjectManager.maxBlgmClans,4}");
        sb.AppendLine($"  Kingdoms: {BLGMObjectManager.BlgmKingdomCount,4} / {BLGMObjectManager.maxBlgmKingdoms,4}");
        
        return sb.ToString();
    }
}
