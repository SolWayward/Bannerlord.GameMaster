using System.Collections.Generic;
using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Settlements;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.SettlementCommands
{
    /// <summary>
    /// Commands that operate specifically on villages
    /// </summary>
    [CommandLineFunctionality.CommandLineArgumentFunction("settlement", "gm")]
    public static class VillageManagementCommands
    {
        /// MARK: set_hearths
        /// <summary>
        /// Set hearth for a village
        /// Usage: gm.settlement.set_hearths [settlement] [value]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_hearths", "gm.settlement")]
        public static string SetHearths(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                var parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("settlement", true),
                    new CommandBase.ArgumentDefinition("value", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                var usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_hearths", "<settlement> <value>",
                    "Sets the hearth value of a village.",
                    "gm.settlement.set_hearths village_1 500");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string settlementQuery = parsed.GetArgument("settlement", 0);
                string valueStr = parsed.GetArgument("value", 1);

                var (settlement, settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                if (settlement.Village == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{settlement.Name}' is not a village.");

                if (!CommandValidator.ValidateFloatRange(valueStr, 0, 2000, out float value, out string valueError))
                    return CommandBase.FormatErrorMessage(valueError);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    float previousValue = settlement.Village.Hearth;
                    settlement.Village.Hearth = value;

                    var resolvedValues = new Dictionary<string, string>
                    {
                        ["settlement"] = settlement.Name.ToString(),
                        ["value"] = value.ToString("F0")
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_hearths", resolvedValues);
                    return display + CommandBase.FormatSuccessMessage(
                        $"Village '{settlement.Name}' (ID: {settlement.StringId}) hearth changed from {previousValue:F0} to {settlement.Village.Hearth:F0}.");
                }, "Failed to set hearths");
            });
        }

        /// MARK: Set Bound 
        /// <summary>
        /// Change which settlement (town/castle) a village is bound to
        /// Usage: gm.settlement.set_village_bound_settlement [village] [settlement]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_village_bound_settlement", "gm.settlement")]
        public static string SetVillageBoundSettlement(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                CommandBase.ParsedArguments parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("village", true),
                    new CommandBase.ArgumentDefinition("settlement", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_village_bound_settlement", "<village> <settlement>",
                    "Changes which settlement (town or castle) a village is bound to.\n" +
                    "- village: Required. Village settlement name or ID (must have Village component)\n" +
                    "- settlement: Required. Town or castle settlement name or ID to bind the village to\n" +
                    "If bound to a town, the village will also be trade-bound to that town.\n" +
                    "If bound to a castle, the command will attempt to automatically set a trade-bound town.\n" +
                    "Changes persist through save/load cycles.\n" +
                    "Supports named arguments: village:village_name settlement:town_name",
                    "gm.settlement.set_village_bound_settlement village_e1 town_e1\n" +
                    "gm.settlement.set_village_bound_settlement 'Village Name' 'Castle Name'\n" +
                    "Important Note: It is not reccomended bind or trade bind a village to a settlment far away from the village. This will cause villagers and etc to travel way too far.");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string villageQuery = parsed.GetArgument("village", 0);
                string settlementQuery = parsed.GetArgument("settlement", 1);

                (Settlement villageSettlement, string villageError) = CommandBase.FindSingleSettlement(villageQuery);
                if (villageError != null) return villageError;

                if (villageSettlement.Village == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{villageSettlement.Name}' is not a village.");

                (Settlement newBoundSettlement, string settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    Village village = villageSettlement.Village;
                    BLGMResult result = village.SetBoundSettlement(newBoundSettlement);

                    Dictionary<string, string> resolvedValues = new()
                    {
                        ["village"] = villageSettlement.Name.ToString(),
                        ["settlement"] = newBoundSettlement.Name.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_village_bound_settlement", resolvedValues);

                    if (result.wasSuccessful)
                        return display + CommandBase.FormatSuccessMessage(result.message);
                    else
                        return display + CommandBase.FormatErrorMessage(result.message);
                }, "Failed to set village bound settlement");
            });
        }

        /// MARK: Set Trade Bound
        /// <summary>
        /// Change which town a village is trade-bound to
        /// Usage: gm.settlement.set_village_trade_bound_settlement [village] [settlement]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("set_village_trade_bound_settlement", "gm.settlement")]
        public static string SetVillageTradeBoundSettlement(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error))
                    return error;

                CommandBase.ParsedArguments parsed = CommandBase.ParseArguments(args);
                parsed.SetValidArguments(
                    new CommandBase.ArgumentDefinition("village", true),
                    new CommandBase.ArgumentDefinition("settlement", true)
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandBase.FormatErrorMessage(validationError);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.settlement.set_village_trade_bound_settlement", "<village> <settlement>",
                    "Changes which town a village is trade-bound to.\n" +
                    "- village: Required. Village settlement name or ID (must have Village component)\n" +
                    "- settlement: Required. Town settlement name or ID (must be a town, not a castle)\n" +
                    "Note: If the village is bound to a town, the game ignores the trade-bound settlement.\n" +
                    "Trade-bound is primarily used when a village is bound to a castle (since castles lack markets).\n" +
                    "Changes persist through save/load cycles.\n" +
                    "Supports named arguments: village:village_name settlement:town_name",
                    "gm.settlement.set_village_trade_bound_settlement village_e1 town_e2\n" +
                    "gm.settlement.set_village_trade_bound_settlement 'Village Name' 'Town Name'\n\n" +
                    "Important Note: It is not reccomended bind or trade bind a village to a settlment far away from the village. This will cause villagers and etc to travel way too far.");

            if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                string villageQuery = parsed.GetArgument("village", 0);
                string settlementQuery = parsed.GetArgument("settlement", 1);

                (Settlement villageSettlement, string villageError) = CommandBase.FindSingleSettlement(villageQuery);
                if (villageError != null) return villageError;

                if (villageSettlement.Village == null)
                    return CommandBase.FormatErrorMessage($"Settlement '{villageSettlement.Name}' is not a village.");

                (Settlement tradeBoundSettlement, string settlementError) = CommandBase.FindSingleSettlement(settlementQuery);
                if (settlementError != null) return settlementError;

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    Village village = villageSettlement.Village;
                    BLGMResult result = village.SetTradeBoundSettlement(tradeBoundSettlement);

                    Dictionary<string, string> resolvedValues = new()
                    {
                        ["village"] = villageSettlement.Name.ToString(),
                        ["settlement"] = tradeBoundSettlement.Name.ToString()
                    };

                    string display = parsed.FormatArgumentDisplay("gm.settlement.set_village_trade_bound_settlement", resolvedValues);

                    if (result.wasSuccessful)
                        return display + CommandBase.FormatSuccessMessage(result.message);
                    else
                        return display + CommandBase.FormatErrorMessage(result.message);
                }, "Failed to set village trade bound settlement");
            });
        }
    }
}