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

namespace Bannerlord.GameMaster.Console.ClanCommands.ClanManagementCommands
{
    /// <summary>
    /// Console command to equip all heroes in a clan with stat-based equipment.
    /// Usage: gm.clan.equip_heroes &lt;clan&gt; [tier] [civilian] [includeNativeHeroes]
    /// </summary>
    public static class EquipClanHeroesCommand
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("equip_heroes", "gm.clan")]
        public static string EquipHeroes(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.equip_heroes", "<clan> [tier] [civilian] [includeNativeHeroes]",
                    "Equips all heroes in a clan with level-appropriate equipment based on their combat skills.\n" +
                    "- clan: required, clan name or ID\n" +
                    "- tier: optional, equipment tier (0+). Native items are tier 0-6, mods may add higher. Defaults to auto based on hero level\n" +
                    "- civilian: optional, also replace civilian equipment (true/false). Defaults to false\n" +
                    "- includeNativeHeroes: optional, if true equips all heroes, if false only BLGM heroes (true/false). Defaults to false\n" +
                    "Supports named arguments: clan:Meroc tier:5 civilian:true includeNativeHeroes:false",
                    "gm.clan.equip_heroes Meroc\n" +
                    "gm.clan.equip_heroes clan:Meroc tier:5\n" +
                    "gm.clan.equip_heroes 'dey Meroc' tier:4 civilian:true\n" +
                    "gm.clan.equip_heroes clan:Meroc includeNativeHeroes:true");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("clan", true, null, "c"),
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
                string clanArg = parsed.GetArgument("clan", 0) ?? parsed.GetNamed("c");
                if (string.IsNullOrWhiteSpace(clanArg))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Clan argument cannot be empty."));

                EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
                if (!clanResult.IsSuccess)
                    return CommandResult.Error(clanResult.Message);
                Clan clan = clanResult.Entity;

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
                    { "clan", clan.Name.ToString() },
                    { "tier", tier >= 0 ? tier.ToString() : "auto" },
                    { "civilian", civilian.ToString().ToLowerInvariant() },
                    { "includeNativeHeroes", includeNativeHeroes.ToString().ToLowerInvariant() }
                };

                BLGMResult result = clan.EquipHeroes(tier, civilian, includeNativeHeroes);

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.clan.equip_heroes", resolvedValues);

                if (!result.IsSuccess)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message));

                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(result.Message));
            }).Log().Message;
        }
    }
}
