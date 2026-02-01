using System;
using System.Collections.Generic;
using System.Text;
using Bannerlord.GameMaster.Bandits;
using Bannerlord.GameMaster.Cultures;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Console.BanditCommands.BanditManagementCommands
{
    /// <summary>
    /// Helper methods for bandit-related console commands.
    /// Contains parsing helpers and query methods for bandit statistics.
    /// Note: GetBanditCountsSummary and AppendBanditTypeCounts should eventually be extracted to Bandits/BanditQueries.cs
    /// </summary>
    public static class BanditCommandHelpers
    {
        #region Parsing Helpers

        /// <summary>
        /// Parses bandit type argument and returns list of matching bandit cultures.
        /// Supports 'all', comma-separated list, culture names, IDs, and flexible naming.
        /// </summary>
        /// <param name="banditTypeArg">The bandit type argument string to parse</param>
        /// <returns>Tuple containing list of cultures and error message (null if successful)</returns>
        public static (List<CultureObject> cultures, string error) ParseBanditCultures(string banditTypeArg)
        {
            List<CultureObject> cultures = new();

            // Handle "all" keyword
            if (banditTypeArg.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                cultures.AddRange(CultureLookup.BanditCultures);
                return (cultures, null);
            }

            // Split by comma for multiple types
            string[] types = banditTypeArg.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < types.Length; i++)
            {
                string type = types[i].Trim();
                CultureObject culture = ParseSingleBanditType(type);

                if (culture == null)
                {
                    return (null, $"Invalid bandit type: '{type}'. Valid types: all, looters, deserters/desert, " +
                        "forest/forest_bandits, mountain/mountain_bandits, sea_raiders/sea, steppe/steppe_bandits, " +
                        "corsairs/southern_pirates");
                }

                // Avoid duplicates
                if (!cultures.Contains(culture))
                {
                    cultures.Add(culture);
                }
            }

            if (cultures.Count == 0)
            {
                return (null, "No valid bandit types specified.");
            }

            return (cultures, null);
        }

        /// <summary>
        /// Parses a single bandit type string and returns the matching CultureObject.
        /// Supports culture names, IDs, and flexible/shortened names.
        /// </summary>
        /// <param name="type">The bandit type string to parse</param>
        /// <returns>The matching CultureObject or null if not found</returns>
        public static CultureObject ParseSingleBanditType(string type)
        {
            string lowerType = type.ToLower();

            // Try exact match first using MBObjectManager
            CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(lowerType);
            if (culture != null && culture.IsBandit)
                return culture;

            // Map common names and shortcuts to bandit cultures
            return lowerType switch
            {
                "looters" or "looter" => CultureLookup.Looters,
                "deserters" or "deserter" => CultureLookup.Deserters,
                "desert" or "desert_bandits" or "desertbandits" => CultureLookup.DesertBandits,
                "forest" or "forest_bandits" or "forestbandits" => CultureLookup.ForestBandits,
                "mountain" or "mountain_bandits" or "mountainbandits" => CultureLookup.MountainBandits,
                "sea" or "sea_raiders" or "searaiders" => CultureLookup.SeaRaiders,
                "steppe" or "steppe_bandits" or "steppebandits" => CultureLookup.SteppeBandits,
                "corsairs" or "corsair" or "southern_pirates" or "pirates" => CultureLookup.Corsairs,
                _ => null
            };
        }

        /// <summary>
        /// Formats a list of bandit cultures into a readable string.
        /// </summary>
        /// <param name="cultures">List of bandit cultures to format</param>
        /// <returns>Comma-separated string of culture names or "None" if empty</returns>
        public static string FormatBanditTypeList(List<CultureObject> cultures)
        {
            if (cultures.Count == 0)
                return "None";

            StringBuilder sb = new();
            for (int i = 0; i < cultures.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(cultures[i].Name.ToString());
            }
            return sb.ToString();
        }

        #endregion

        #region Query Methods (TODO: Extract to Bandits/BanditQueries.cs)

        /// <summary>
        /// Gets a formatted summary of all bandit party and hideout counts.
        /// TODO: This method should be extracted to Bandits/BanditQueries.cs
        /// </summary>
        /// <returns>Formatted string containing all bandit statistics</returns>
        public static string GetBanditCountsSummary()
        {
            StringBuilder sb = new();

            // Total counts
            int totalParties = BanditManager.TotalBanditPartyCount;
            int totalHideouts = BanditManager.TotalHideoutCount;

            sb.AppendLine($"Total Parties: {totalParties}");
            sb.AppendLine($"Total Hideouts: {totalHideouts}");
            sb.AppendLine();

            // Individual counts by type
            sb.AppendLine("By Type:");
            AppendBanditTypeCounts(sb, "Looters", BanditManager.LootersPartyCount, BanditManager.LooterHideoutCount);
            AppendBanditTypeCounts(sb, "Deserters", BanditManager.DeserterPartyCount, BanditManager.DeserterHideoutCount);
            AppendBanditTypeCounts(sb, "Desert Bandits", BanditManager.DesertBanditPartyCount, BanditManager.DesertBanditHideoutCount);
            AppendBanditTypeCounts(sb, "Forest Bandits", BanditManager.ForestBanditPartyCount, BanditManager.ForestBanditHideoutCount);
            AppendBanditTypeCounts(sb, "Mountain Bandits", BanditManager.MountainBanditPartyCount, BanditManager.MountainBanditHideoutCount);
            AppendBanditTypeCounts(sb, "Sea Raiders", BanditManager.SeaRaiderPartyCount, BanditManager.SeaRaiderHideoutCount);
            AppendBanditTypeCounts(sb, "Steppe Bandits", BanditManager.SteppeBanditPartyCount, BanditManager.SteppeBanditHideoutCount);

            // Only include Corsairs if Warsails DLC is loaded
            if (GameEnvironment.IsWarsailsDlcLoaded)
            {
                AppendBanditTypeCounts(sb, "Corsairs", BanditManager.CorsairPartyCount, BanditManager.CorsairHideoutCount);
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Appends formatted count information for a specific bandit type.
        /// TODO: This method should be extracted to Bandits/BanditQueries.cs
        /// </summary>
        /// <param name="sb">StringBuilder to append to</param>
        /// <param name="typeName">Display name of the bandit type</param>
        /// <param name="partyCount">Number of parties of this type</param>
        /// <param name="hideoutCount">Number of hideouts of this type</param>
        private static void AppendBanditTypeCounts(StringBuilder sb, string typeName, int partyCount, int hideoutCount)
        {
            sb.AppendLine($"  {typeName}: {partyCount} parties, {hideoutCount} hideouts");
        }

        #endregion
    }
}
