using Bannerlord.GameMaster.Caravans;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.CaravanCommands.CaravanCreationCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("caravan", "gm")]
    public static class CreateNotableCaravanCommand
    {
        /// <summary>
        /// Create a caravan in a settlement for notables.
        /// Usage: gm.caravan.create_notable_caravan [settlement]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_notable_caravan", "gm.caravan")]
        public static string CreateNotableCaravan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error).Message
;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.caravan.create_notable_caravan", "<settlement>",
                    "Creates a new caravan in the specified settlement owned by a notable who doesn't have one yet.",
                    "gm.caravan.create_notable_caravan pen");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);
                parsed.SetValidArguments(
                    new ArgumentDefinition("settlement", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Message
;

                if (parsed.TotalCount < 1)
                    return usageMessage;

                // MARK: Parse Arguments
                string settlementQuery = parsed.GetArgument("settlement", 0);
                if (string.IsNullOrWhiteSpace(settlementQuery))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Settlement cannot be empty.")).Message
;

                EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
                if (!settlementResult.IsSuccess) return settlementResult.Message;
                Settlement settlement = settlementResult.Entity;

                if (!settlement.IsTown)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city. Caravans can only be created in cities.")).Message
;

                // MARK: Execute Logic
                MobileParty caravan = CaravanManager.CreateNotableCaravan(settlement);

                if (caravan == null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Failed to create notable caravan in '{settlement.Name}'. " +
                        $"All notables may already own caravans, or no suitable notable was found.")).Message
;

                Dictionary<string, string> resolvedValues = new()
                {
                    ["settlement"] = settlement.Name.ToString()
                };

                string display = parsed.FormatArgumentDisplay("gm.caravan.create_notable_caravan", resolvedValues);
                return CommandResult.Success(display + MessageFormatter.FormatSuccessMessage(
                    $"Created caravan in '{settlement.Name}' (ID: {settlement.StringId}) owned by notable {caravan.Owner?.Name}.")).Message
;
            });
        }
    }
}
