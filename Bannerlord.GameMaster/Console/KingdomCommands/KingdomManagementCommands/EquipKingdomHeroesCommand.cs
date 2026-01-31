using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Factions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.KingdomCommands.KingdomManagementCommands
{
    /// <summary>
    /// Console command to equip all heroes in a kingdom with stat-based equipment.
    /// Usage: gm.kingdom.equip_heroes &lt;kingdom&gt; [tier] [civilian] [includeNativeHeroes]
    /// </summary>
    public static class EquipKingdomHeroesCommand
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("equip_heroes", "gm.kingdom")]
        public static string EquipHeroes(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.kingdom.equip_heroes", "<kingdom> [tier] [civilian] [includeNativeHeroes]",
                    "Equips all heroes in a kingdom with level-appropriate equipment based on their combat skills.\n" +
                    "- kingdom: required, kingdom name or ID\n" +
                    "- tier: optional, equipment tier (0+). Native items are tier 0-6, mods may add higher. Defaults to auto based on hero level\n" +
                    "- civilian: optional, also replace civilian equipment (true/false). Defaults to false\n" +
                    "- includeNativeHeroes: optional, if true equips all heroes, if false only BLGM heroes (true/false). Defaults to false\n" +
                    "Supports named arguments: kingdom:vlandia tier:5 civilian:true includeNativeHeroes:false",
                    "gm.kingdom.equip_heroes vlandia\n" +
                    "gm.kingdom.equip_heroes kingdom:vlandia tier:5\n" +
                    "gm.kingdom.equip_heroes 'Northern Empire' tier:4 civilian:true\n" +
                    "gm.kingdom.equip_heroes kingdom:empire_w includeNativeHeroes:true");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("kingdom", true, null, "k"),
                    new ArgumentDefinition("tier", false, "auto", "t"),
                    new ArgumentDefinition("civilian", false, "false", "civ"),
                    new ArgumentDefinition("includeNativeHeroes", false, "false", "native", "include")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

                if (parsed.TotalCount < 1)
                    return CommandResult.Success(usageMessage);

                // MARK: Parse Arguments
                string kingdomArg = parsed.GetArgument("kingdom", 0) ?? parsed.GetNamed("k");
                if (string.IsNullOrWhiteSpace(kingdomArg))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Kingdom argument cannot be empty."));

                EntityFinderResult<Kingdom> kingdomResult = KingdomFinder.FindSingleKingdom(kingdomArg);
                if (!kingdomResult.IsSuccess)
                    return CommandResult.Error(kingdomResult.Message);
                Kingdom kingdom = kingdomResult.Entity;

                // Parse tier
                int tier = -1; // -1 means auto-calculate from hero level
                string tierArg = parsed.GetNamed("tier") ?? parsed.GetNamed("t");
                if (tierArg == null && parsed.PositionalCount > 1)
                {
                    string secondArg = parsed.GetPositional(1);
                    if (int.TryParse(secondArg, out int _))
                        tierArg = secondArg;
                }

                if (tierArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(tierArg, 0, int.MaxValue, out tier, out string tierError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(tierError));
                }

                // Parse civilian
                bool civilian = false;
                string civilianArg = parsed.GetNamed("civilian") ?? parsed.GetNamed("civ");
                if (civilianArg != null)
                {
                    civilian = civilianArg.ToLowerInvariant() == "true";
                }
                else
                {
                    // Scan positional arguments for boolean
                    for (int i = 1; i < parsed.PositionalCount; i++)
                    {
                        string posArg = parsed.GetPositional(i)?.ToLowerInvariant();
                        if (posArg == "true" || posArg == "false")
                        {
                            civilian = posArg == "true";
                            break;
                        }
                    }
                }

                // Parse includeNativeHeroes
                bool includeNativeHeroes = false;
                string includeArg = parsed.GetNamed("includeNativeHeroes")
                    ?? parsed.GetNamed("native")
                    ?? parsed.GetNamed("include");
                if (includeArg != null)
                {
                    includeNativeHeroes = includeArg.ToLowerInvariant() == "true";
                }

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "kingdom", kingdom.Name.ToString() },
                    { "tier", tier >= 0 ? tier.ToString() : "auto" },
                    { "civilian", civilian.ToString().ToLowerInvariant() },
                    { "includeNativeHeroes", includeNativeHeroes.ToString().ToLowerInvariant() }
                };

                BLGMResult result = kingdom.EquipHeroes(tier, civilian, includeNativeHeroes);

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.kingdom.equip_heroes", resolvedValues);

                if (!result.IsSuccess)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message));

                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message));
            }).Log().Message;
        }
    }
}
