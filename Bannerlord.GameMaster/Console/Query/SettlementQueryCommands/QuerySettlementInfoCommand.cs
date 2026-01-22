using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Settlements;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.Query.SettlementQueryCommands;

/// <summary>
/// Get detailed info about a specific settlement by ID
/// Usage: gm.query.settlement_info [settlementId]
/// Example: gm.query.settlement_info town_empire_1
/// </summary>
public static class QuerySettlementInfoCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("settlement_info", "gm.query")]
    public static string Execute(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error).Log().Message;

            if (args == null || args.Count == 0)
                return CommandResult.Error("Please provide a settlement ID.\nUsage: gm.query.settlement_info <settlementId>\n").Log().Message;

            // MARK: Parse Arguments
            string settlementId = args[0];
            Settlement settlement = SettlementQueries.GetSettlementById(settlementId);

            if (settlement == null)
                return CommandResult.Error($"Settlement with ID '{settlementId}' not found.\n").Log().Message;

            // MARK: Execute Logic
            SettlementTypes types = settlement.GetSettlementTypes();
            string settlementType = settlement.IsTown
                ? (settlement.IsCastle ? "Castle" : "City")
                : settlement.IsVillage ? "Village"
                : settlement.IsHideout ? "Hideout"
                : "Unknown";

            string ownerName = settlement.OwnerClan?.Name?.ToString() ?? "None";
            string kingdomName = settlement.MapFaction?.Name?.ToString() ?? "None";
            string cultureName = settlement.Culture?.Name?.ToString() ?? "None";

            string prosperityInfo = "";
            if (settlement.IsTown && settlement.Town != null)
            {
                prosperityInfo = $"Prosperity: {settlement.Town.Prosperity:F0}\n" +
                               $"Security: {settlement.Town.Security:F0}\n" +
                               $"Loyalty: {settlement.Town.Loyalty:F0}\n" +
                               $"Food Stocks: {settlement.Town.FoodStocks:F0}\n";
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                prosperityInfo = $"Hearth: {settlement.Village.Hearth:F0}\n" +
                               $"Bound Town: {settlement.Village.Bound?.Name}\n";
            }

            string siegeInfo = settlement.IsUnderSiege
                ? $"Under Siege: Yes\nBesieger: {settlement.SiegeEvent?.BesiegerCamp?.LeaderParty?.Name}\n"
                : "Under Siege: No\n";

            string notableInfo = "";
            if (settlement.Notables != null && settlement.Notables.Any())
            {
                notableInfo = $"Notables: {settlement.Notables.Count()}\n";
            }

            return CommandResult.Success($"Settlement Information:\n" +
                   $"ID: {settlement.StringId}\n" +
                   $"Name: {settlement.Name}\n" +
                   $"Type: {settlementType}\n" +
                   $"Owner: {ownerName}\n" +
                   $"Kingdom: {kingdomName}\n" +
                   $"Culture: {cultureName}\n" +
                   $"{prosperityInfo}" +
                   $"{siegeInfo}" +
                   $"{notableInfo}" +
                   $"Types: {types}\n" +
                   $"Position: X={settlement.GetPosition2D.X:F1}, Y={settlement.GetPosition2D.Y:F1}\n").Log().Message;
        });
    }
}
