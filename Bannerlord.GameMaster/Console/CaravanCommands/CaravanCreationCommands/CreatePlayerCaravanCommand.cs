using Bannerlord.GameMaster.Caravans;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CaravanCommands.CaravanCreationCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("caravan", "gm")]
    public static class CreatePlayerCaravanCommand
    {
        /// <summary>
        /// Create a caravan in a settlement for the player.
        /// Usage: gm.caravan.create_player_caravan [settlement] [optional: leader_hero]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_player_caravan", "gm.caravan")]
        public static string CreatePlayerCaravan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.create_player_caravan", "<settlement> [leader_hero]",
                    "Creates a new caravan for the player's clan. Optionally specify a companion hero to lead it.\n" +
                    "- settlement: required, town settlement where caravan will be created\n" +
                    "- leader/leaderHero: optional, companion hero ID or name to lead caravan (auto-selected if omitted)",
                    "gm.caravan.create_player_caravan pen\n" +
                    "gm.caravan.create_player_caravan pen companion_hero\n" +
                    "gm.caravan.create_player_caravan settlement:pen leader:myCompanion");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);
                parsed.SetValidArguments(
                    new ArgumentDefinition("settlement", true),
                    new ArgumentDefinition("leader", false, "Auto-selected", "leaderHero")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return MessageFormatter.FormatErrorMessage(validationError);

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string settlementQuery = parsed.GetArgument("settlement", 0);
                if (string.IsNullOrWhiteSpace(settlementQuery))
                    return MessageFormatter.FormatErrorMessage("Settlement cannot be empty.");

                EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
                if (!settlementResult.IsSuccess) return settlementResult.Message;
                Settlement settlement = settlementResult.Entity;

                if (!settlement.IsTown)
                    return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city. Caravans can only be created in cities.");

                Hero caravanLeader = null;
                string leaderQuery = parsed.GetArgument("leader", 1) ?? parsed.GetArgument("leaderHero", 1);

                if (!string.IsNullOrEmpty(leaderQuery) && leaderQuery.ToLower() != "null")
                {
                    EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(leaderQuery);
                    if (!heroResult.IsSuccess) return heroResult.Message;

                    if (heroResult.Entity.Clan != Clan.PlayerClan)
                        return MessageFormatter.FormatErrorMessage($"{heroResult.Entity.Name} is not a member of the player's clan.");

                    if (heroResult.Entity.PartyBelongedTo != null)
                        return MessageFormatter.FormatErrorMessage($"{heroResult.Entity.Name} is already in a party.");

                    caravanLeader = heroResult.Entity;
                }

                // MARK: Execute Logic
                MobileParty caravan = CaravanManager.CreatePlayerCaravan(settlement, caravanLeader);

                if (caravan == null)
                    return MessageFormatter.FormatErrorMessage($"Failed to create player caravan in '{settlement.Name}'.");

                string leaderInfo = caravanLeader != null ? $" led by {caravanLeader.Name}" : " (no leader assigned)";

                Dictionary<string, string> resolvedValues = new()
                {
                    ["settlement"] = settlement.Name.ToString(),
                    ["leader"] = caravanLeader?.Name?.ToString() ?? "Auto-selected"
                };

                string display = parsed.FormatArgumentDisplay("create_player_caravan", resolvedValues);
                return display + MessageFormatter.FormatSuccessMessage(
                    $"Created player caravan in '{settlement.Name}' (ID: {settlement.StringId}){leaderInfo}.\n" +
                    $"The caravan will generate trade profits for your clan.");
            });
        }
    }
}
