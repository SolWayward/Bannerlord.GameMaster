using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Party;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands;

/// <summary>
/// Opens the native party editor UI for a hero's party, optionally with a second hero's party
/// for troop transfer. If no second hero is specified, opens in discard mode with all game troops.
/// Usage: gm.hero.edit_party &lt;rightSideHero&gt; [leftSideHero]
/// </summary>
public static class EditPartyCommand
{
    [CommandLineFunctionality.CommandLineArgumentFunction("edit_party", "gm.hero")]
    public static string EditParty(List<string> args)
    {
        return Cmd.Run(args, () =>
        {
            // MARK: Validation
            if (!CommandValidator.ValidateCampaignState(out string error))
                return CommandResult.Error(error);

            string usageMessage = CommandValidator.CreateUsageMessage(
                "gm.hero.edit_party", "<rightSideHero> [leftSideHero]",
                "Opens the native party editor screen for the specified hero's party.\n" +
                "- rightSideHero/hero: required, hero ID or name. Their party appears on the right side.\n" +
                "- leftSideHero/other: optional, hero ID or name. Their party appears on the left side for troop transfer.\n" +
                "  If not specified, the left side is populated with all game troops (100 each) as a discard roster.\n" +
                "Supports named arguments: hero:'Hero Name' other:'Other Hero'",
                "gm.hero.edit_party player\n" +
                "gm.hero.edit_party 'Ira of the Vaegir'\n" +
                "gm.hero.edit_party player derthert" +
                "gm.hero.edit_party hero:lord_1_1\n" +
                "gm.hero.edit_party player lord_4_1\n" +
                "gm.hero.edit_party hero:player other:'Ira of the Vaegir'");

            ParsedArguments parsed = ArgumentParser.ParseArguments(args);

            parsed.SetValidArguments(
                new ArgumentDefinition("rightSideHero", true, null, "hero"),
                new ArgumentDefinition("leftSideHero", false, "Discard Roster", "other")
            );

            string validationError = parsed.GetValidationError();
            if (validationError != null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

            if (parsed.TotalCount < 1)
                return CommandResult.Success(usageMessage);

            // MARK: Parse Arguments
            string rightHeroArg = parsed.GetArgument("rightSideHero", 0) ?? parsed.GetNamed("hero");
            if (string.IsNullOrWhiteSpace(rightHeroArg))
                return CommandResult.Error(MessageFormatter.FormatErrorMessage("Missing required argument 'rightSideHero'."));

            EntityFinderResult<Hero> rightHeroResult = HeroFinder.FindSingleHero(rightHeroArg);
            if (!rightHeroResult.IsSuccess)
                return CommandResult.Error(rightHeroResult.Message);
            Hero rightHero = rightHeroResult.Entity;

            if (rightHero.PartyBelongedTo == null)
                return CommandResult.Error(MessageFormatter.FormatErrorMessage($"{rightHero.Name} is not in a party."));

            // Parse optional leftSideHero - only pass null if not specified at all
            Hero leftHero = null;
            string leftHeroArg = parsed.GetArgument("leftSideHero", 1) ?? parsed.GetNamed("other");
            if (leftHeroArg != null)
            {
                EntityFinderResult<Hero> leftHeroResult = HeroFinder.FindSingleHero(leftHeroArg);
                if (!leftHeroResult.IsSuccess)
                    return CommandResult.Error(leftHeroResult.Message);
                leftHero = leftHeroResult.Entity;
            }

            // MARK: Execute Logic
            Dictionary<string, string> resolvedValues = new()
            {
                { "rightSideHero", rightHero.Name.ToString() },
                { "leftSideHero", leftHero != null ? leftHero.Name.ToString() : "Discard Roster" }
            };

            BLGMResult result = PartyManager.OpenPartyEditor(rightHero, leftHero);

            if (result == null || !result.IsSuccess)
            {
                string errorMsg = result?.Message ?? "Failed to open party editor";
                return CommandResult.Error(MessageFormatter.FormatErrorMessage(errorMsg));
            }

            string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.edit_party", resolvedValues);
            string modeInfo = leftHero != null
                ? $"Left side: {leftHero.Name}'s party | Right side: {rightHero.Name}'s party"
                : $"Left side: All game troops (discard) | Right side: {rightHero.Name}'s party";

            return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(
                $"Opened party editor for {rightHero.Name} (ID: {rightHero.StringId})\n" +
                modeInfo));
        }).Message;
    }
}
