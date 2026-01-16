using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands.FortificationManagementCommands;

/// <summary>
/// Command to fill garrison to maximum capacity using existing troop types.
/// Usage: gm.settlement.fill_garrison [settlement]
/// </summary>
public static class FillGarrisonCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("fill_garrison", "gm.settlement")]
    public static string FillGarrison(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return error;

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.settlement.fill_garrison", "<settlement>",
                "Fills the garrison to maximum capacity using a mix of troops already present.",
                "gm.settlement.fill_garrison pen");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);
            parsed.SetValidArguments(
                new ArgumentDefinition("settlement", true)
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return MessageFormatter.FormatErrorMessage(validationError);

            if (parsed.TotalCount < 1)
                return usageMessage;

            // MARK: Parse Arguments
            string settlementQuery = parsed.GetArgument("settlement", 0);

            EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementQuery);
            if (!settlementResult.IsSuccess) return settlementResult.Message;
            Settlement settlement = settlementResult.Entity;

            if (settlement.Town == null)
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' is not a city or castle.");

            if (settlement.Town.GarrisonParty == null)
                return MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' has no garrison party.");

            // MARK: Execute Logic
            TaleWorlds.CampaignSystem.Party.MobileParty garrison = settlement.Town.GarrisonParty;
            int currentSize = garrison.MemberRoster.TotalManCount;
            int maxSize = garrison.Party.PartySizeLimit;
            int spaceAvailable = maxSize - currentSize;

            Dictionary<string, string> resolvedValues = new()
            {
                ["settlement"] = settlement.Name.ToString()
            };

            string argumentDisplay = parsed.FormatArgumentDisplay("fill_garrison", resolvedValues);

            if (spaceAvailable <= 0)
                return argumentDisplay + MessageFormatter.FormatSuccessMessage($"Settlement '{settlement.Name}' garrison is already at maximum capacity ({currentSize}/{maxSize}).");

            // Get existing troops and their proportions
            List<(CharacterObject troop, int count)> troopTypes = new();
            foreach (TaleWorlds.CampaignSystem.Roster.TroopRosterElement element in garrison.MemberRoster.GetTroopRoster())
            {
                if (element.Character != null && !element.Character.IsHero)
                {
                    troopTypes.Add((element.Character, element.Number));
                }
            }

            if (troopTypes.Count == 0)
                return argumentDisplay + MessageFormatter.FormatErrorMessage($"Settlement '{settlement.Name}' has no troops to use as template for filling.");

            // Calculate total existing troops for proportions
            int totalExisting = troopTypes.Sum(t => t.count);

            // Add troops proportionally
            int addedCount = 0;
            foreach ((CharacterObject troop, int count) in troopTypes)
            {
                // Calculate proportion of this troop type
                float proportion = (float)count / totalExisting;
                int toAdd = (int)(spaceAvailable * proportion);

                if (toAdd > 0)
                {
                    garrison.MemberRoster.AddToCounts(troop, toAdd);
                    addedCount += toAdd;
                }
            }

            // Add any remaining space with the most common troop
            int remaining = spaceAvailable - addedCount;
            if (remaining > 0)
            {
                (CharacterObject troop, int count) mostCommon = troopTypes.OrderByDescending(t => t.count).First();
                garrison.MemberRoster.AddToCounts(mostCommon.troop, remaining);
                addedCount += remaining;
            }

            return argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Settlement '{settlement.Name}' (ID: {settlement.StringId}) garrison filled from {currentSize} to {garrison.MemberRoster.TotalManCount} (+{addedCount}).");
        });
    }
}
