using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Bannerlord.GameMaster.Heroes;
using Bannerlord.GameMaster.Clans;
using Bannerlord.GameMaster.Kingdoms;

namespace Bannerlord.GameMaster.Console.Common
{
    /// <summary>
    /// Base class providing common functionality for all command classes
    /// </summary>
    public static class CommandBase
    {
        #region Entity Finder Methods

    /// <summary>
 /// Helper method to find a single hero from a query
        /// </summary>
        public static (Hero hero, string error) FindSingleHero(string query)
        {
    List<Hero> matchedHeroes = HeroQueries.FindHeroes(query);

       if (matchedHeroes == null || matchedHeroes.Count == 0)
                return (null, $"Error: No hero matching query '{query}' found.\n");

            if (matchedHeroes.Count > 1)
            {
                return (null, $"Error: Found {matchedHeroes.Count} heroes matching query '{query}':\n" +
$"{HeroQueries.GetFormattedDetails(matchedHeroes)}" +
        $"Please use a more specific name or ID.\n");
      }

            return (matchedHeroes[0], null);
        }

 /// <summary>
  /// Helper method to find a single clan from a query
        /// </summary>
        public static (Clan clan, string error) FindSingleClan(string query)
     {
            List<Clan> matchedClans = ClanQueries.FindClans(query);

            if (matchedClans == null || matchedClans.Count == 0)
       return (null, $"Error: No clan matching query '{query}' found.\n");

         if (matchedClans.Count > 1)
            {
     return (null, $"Error: Found {matchedClans.Count} clans matching query '{query}':\n" +
          $"{ClanQueries.GetFormattedDetails(matchedClans)}" +
        $"Please use a more specific name or ID.\n");
            }

      return (matchedClans[0], null);
        }

        /// <summary>
        /// Helper method to find a single kingdom from a query
        /// </summary>
        public static (Kingdom kingdom, string error) FindSingleKingdom(string query)
        {
            List<Kingdom> matchedKingdoms = KingdomQueries.FindKingdoms(query);

   if (matchedKingdoms == null || matchedKingdoms.Count == 0)
      return (null, $"Error: No kingdom matching query '{query}' found.\n");

      if (matchedKingdoms.Count > 1)
      {
 return (null, $"Error: Found {matchedKingdoms.Count} kingdoms matching query '{query}':\n" +
             $"{KingdomQueries.GetFormattedDetails(matchedKingdoms)}" +
   $"Please use a more specific name or ID.\n");
      }

            return (matchedKingdoms[0], null);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates campaign mode
        /// </summary>
        public static bool ValidateCampaignMode(out string error)
        {
     if (Campaign.Current == null)
      {
   error = "Error: Must be in campaign mode.\n";
   return false;
      }
            error = null;
            return true;
      }

        /// <summary>
        /// Validates minimum argument count
    /// </summary>
        public static bool ValidateArgumentCount(List<string> args, int requiredCount, string usageMessage, out string error)
        {
            if (args == null || args.Count < requiredCount)
   {
           error = $"Error: Missing arguments.\n{usageMessage}";
     return false;
      }
            error = null;
            return true;
        }

        #endregion

        #region Helper Methods

  /// <summary>
     /// Executes an action safely with consistent error handling
        /// </summary>
      public static string ExecuteWithErrorHandling(Func<string> action, string errorPrefix = "Error")
        {
            try
      {
       return action();
   }
        catch (Exception ex)
 {
    return $"{errorPrefix}: {ex.Message}\n";
          }
        }

        /// <summary>
    /// Formats a success message with consistent styling
  /// </summary>
      public static string FormatSuccessMessage(string message)
        {
       return $"Success: {message}\n";
        }

 /// <summary>
        /// Formats an error message with consistent styling
        /// </summary>
        public static string FormatErrorMessage(string message)
        {
            return $"Error: {message}\n";
        }

        #endregion
    }
}