using System.Collections.Generic;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Kingdoms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomDiplomacyCommands;

public static class GetTributeInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("get_tribute_info", "gm.kingdom")]
    public static string GetTributeInfo(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.kingdom.get_tribute_info", "<kingdomA> <kingdomB>",
                "Displays tribute information between two kingdoms.\n" +
                "Supports named arguments: kingdomA:empire kingdomB:battania",
                "gm.kingdom.get_tribute_info empire battania");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("kingdomA", true),
                new ArgumentDefinition("kingdomB", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError)).Log().Message;

            if (parsed.TotalCount < 2)
                return usageMessage;

            // MARK: Parse Arguments
            string kingdomAArg = parsed.GetArgument("kingdomA", 0);
            if (kingdomAArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'kingdomA'.");

            EntityFinderResult<Kingdom> kingdomAResult = KingdomFinder.FindSingleKingdom(kingdomAArg);
            if (!kingdomAResult.IsSuccess) return kingdomAResult.Message;
            Kingdom kingdomA = kingdomAResult.Entity;

            string kingdomBArg = parsed.GetArgument("kingdomB", 1);
            if (kingdomBArg == null)
                return MessageFormatter.FormatErrorMessage("Missing required argument 'kingdomB'.");

            EntityFinderResult<Kingdom> kingdomBResult = KingdomFinder.FindSingleKingdom(kingdomBArg);
            if (!kingdomBResult.IsSuccess) return kingdomBResult.Message;
            Kingdom kingdomB = kingdomBResult.Entity;

            if (kingdomA == kingdomB)
                return MessageFormatter.FormatErrorMessage("Cannot get tribute info for a kingdom with itself.");

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "kingdomA", kingdomA.Name.ToString() },
                { "kingdomB", kingdomB.Name.ToString() }
            };
            string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.get_tribute_info", resolvedValues);

            TributeInfo tributeInfo = kingdomA.GetTributeInfo(kingdomB);
            string tributeString = tributeInfo.GetTributeString();

            return argumentDisplay + tributeString + "\n";
        });
    }
}
