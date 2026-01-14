using System.Collections.Generic;
using Bannerlord.GameMaster.Caravans;
using Bannerlord.GameMaster.Console.Common;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CaravanCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("caravan", "gm")]
    public static class CaravanCreationCommands
    {
        #region Commands

        /// MARK: create_notable_caravan
        /// <summary>
        /// Create a caravan in a settlement for notables
        /// Usage: gm.caravan.create_notable_caravan [settlement]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_notable_caravan", "gm.caravan")]
        public static string CreateNotableCaravan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                CommandBase.ParsedArguments parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.create_notable_caravan", "<settlement>",
                    "Creates a new caravan in the specified settlement owned by a notable who doesn't have one yet.",
                    "gm.caravan.create_notable_caravan pen");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);

                (Settlement settlement, string settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (!settlement.IsTown)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city. Caravans can only be created in cities.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    MobileParty caravan = CaravanManager.CreateNotableCaravan(settlement);

                    if (caravan == null)
                        return CommandBase.FormatErrorMessage($"Failed to create notable caravan in '{settlement.Name}'. " +
                            $"All notables may already own caravans, or no suitable notable was found.");

                    Dictionary<string, string> resolvedValues = new()
                    {
                        ["settlement"] = settlement.Name.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.caravan.create_notable_caravan", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Created caravan in '{settlement.Name}' (ID: {settlement.StringId}) owned by notable {caravan.Owner?.Name}.");
                }, "Failed to create notable caravan");
            });
        }

        /// MARK: create_player_caravan
        /// <summary>
        /// Create a caravan in a settlement for the player
        /// Usage: gm.caravan.create_player_caravan [settlement] [optional: leader_hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_player_caravan", "gm.caravan")]
        public static string CreatePlayerCaravan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                CommandBase.ParsedArguments parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("leader", false, "Auto-selected")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.create_player_caravan", "<settlement> [leader_hero]",
                    "Creates a new caravan for the player's clan. Optionally specify a companion hero to lead it.",
                    "gm.caravan.create_player_caravan pen\ngm.caravan.create_player_caravan pen companion_hero");

                if (!CommandBase.ValidateArgumentCount(args, 1, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string leaderQuery = parsed.GetArgument("leader", 1);

                (Settlement settlement, string settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (!settlement.IsTown)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city. Caravans can only be created in cities.");

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    Hero caravanLeader = null;
                    
                    // Check if a specific leader was requested
                    if (!string.IsNullOrEmpty(leaderQuery))
                    {
                        (Hero hero, string heroError) = CommandBase.FindSingleHero(leaderQuery);
                        if (heroError != null) return heroError;
                        
                        if (hero.Clan != Clan.PlayerClan)
                            return CommandBase.FormatErrorMessage($"{hero.Name} is not a member of the player's clan.");
                        
                        if (hero.PartyBelongedTo != null)
                            return CommandBase.FormatErrorMessage($"{hero.Name} is already in a party.");
                        
                        caravanLeader = hero;
                    }

                    MobileParty caravan = CaravanManager.CreatePlayerCaravan(settlement, caravanLeader);

                    if (caravan == null)
                        return CommandBase.FormatErrorMessage($"Failed to create player caravan in '{settlement.Name}'.");

                    string leaderInfo = caravanLeader != null ? $" led by {caravanLeader.Name}" : " (no leader assigned)";
                    
                    Dictionary<string, string> resolvedValues = new()
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["leader"] = caravanLeader?.Name?.ToString() ?? "Auto-selected"
                    };

                    string display = parsed.FormatArgumentDisplay("gm.caravan.create_player_caravan", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Created player caravan in '{settlement.Name}' (ID: {settlement.StringId}){leaderInfo}.\n" +
                        $"The caravan will generate trade profits for your clan.");
                }, "Failed to create player caravan");
            });
        }

        #endregion
    }
}
