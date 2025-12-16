using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Troops;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.TroopCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("troops", "gm")]
    public static class TroopManagementCommands
    {
        /// <summary>
        /// Give troops to a hero's party
        /// Usage: gm.troops.give_hero_troops [heroQuery] [troopQuery] [count]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("give_hero_troops", "gm.troops")]
        public static string GiveHeroTroops(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // Validate campaign mode
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                // Create usage message
                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.troops.give_hero_troops", "<hero_query> <character_query> <count>",
                    "Gives characters/troops to a hero's party. Accepts any CharacterObject including dancers, refugees, etc.",
                    "gm.troops.give_hero_troops player imperial_recruit 10");

                // Validate argument count (exactly 3)
                if (!CommandBase.ValidateArgumentCount(args, 3, usageMessage, out error))
                    return error;

                // Find hero
                var (hero, heroError) = CommandBase.FindSingleHero(args[0]);
                if (heroError != null) return heroError;

                // Find character (accepts any CharacterObject including dancers, refugees, etc.)
                var (character, characterError) = CommandBase.FindSingleCharacterObject(args[1]);
                if (characterError != null) return characterError;

                // Validate character is not a hero
                if (character.IsHero)
                    return CommandBase.FormatErrorMessage($"{character.Name} is a hero and cannot be added as a troop.");

                // Validate count (1 to 10000)
                if (!CommandValidator.ValidateIntegerRange(args[2], 1, 10000, out int count, out string countError))
                    return CommandBase.FormatErrorMessage(countError);

                // Execute the operation with error handling
                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    // Check hero has a party
                    if (hero.PartyBelongedTo == null)
                        return CommandBase.FormatErrorMessage($"{hero.Name} does not belong to a party.");

                    // Check if hero is the party leader
                    bool isPartyLeader = hero.PartyBelongedTo.LeaderHero == hero;
                    
                    // Add character/troop to party
                    hero.PartyBelongedTo.MemberRoster.AddToCounts(character, count);

                    // Build success message
                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Added {count}x {character.Name} to {hero.Name}'s party.");
                    
                    // Include party info (especially if hero is not the leader)
                    if (!isPartyLeader)
                    {
                        result.AppendLine($"Party: {hero.PartyBelongedTo.Name} (Leader: {hero.PartyBelongedTo.LeaderHero.Name})");
                    }
                    else
                    {
                        result.AppendLine($"Party: {hero.PartyBelongedTo.Name}");
                    }

                    return CommandBase.FormatSuccessMessage(result.ToString());
                }, "Failed to give troops");
            });
        }
    }
}