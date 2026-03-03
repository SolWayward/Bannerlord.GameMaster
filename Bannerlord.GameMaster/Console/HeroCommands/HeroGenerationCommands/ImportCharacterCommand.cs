using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Heroes;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroGenerationCommands
{
    /// <summary>
    /// Console command to import a character set file and create a new hero from it.
    /// </summary>
    public static class ImportCharacterCommand
    {
        /// <summary>
        /// Import a character set file to create a new hero.
        /// Usage: gm.hero.import_character &lt;filename&gt; &lt;clan&gt; [type] [settlement] [withParty]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("import_character", "gm.hero")]
        public static string ImportCharacter(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.import_character", "<filename> <clan> [type] [settlement] [withParty]",
                    "Creates a new hero from a previously exported character set file.\n" +
                    "The saved appearance, development, traits, and equipment are applied to the new hero.\n" +
                    "- filename: required, character set filename (without extension)\n" +
                    "- clan: required, target clan name, stringId, or partial match\n" +
                    "- type: optional, override hero type: lord, wanderer, companion. Defaults to saved type\n" +
                    "- settlement: optional, placement settlement name or ID. Defaults to auto-resolved from clan\n" +
                    "- withParty/party: optional, for lords: create party (true/false). Defaults to true\n" +
                    "Supports named arguments: filename:my_king clan:meroc type:lord settlement:Poros withParty:true\n",
                    "gm.hero.import_character my_king 'dey meroc'\n" +
                    "gm.hero.import_character king_export meroc lord\n" +
                    "gm.hero.import_character filename:ira_backup clan:meroc type:companion\n" +
                    "gm.hero.import_character ira_backup 'dey meroc' wanderer Poros\n" +
                    "gm.hero.import_character ira_backup meroc lord 'Vladiv Castle' false");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("filename", true),
                    new ArgumentDefinition("clan", true),
                    new ArgumentDefinition("type", false),
                    new ArgumentDefinition("settlement", false),
                    new ArgumentDefinition("withParty", false, null, "party")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

                if (parsed.TotalCount < 2)
                    return CommandResult.Success(usageMessage);

                // MARK: Parse Arguments
                string filenameArg = parsed.GetArgument("filename", 0);
                if (string.IsNullOrWhiteSpace(filenameArg))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Filename argument cannot be empty."));

                // Verify file exists
                CharacterSetFileManager fileManager = CharacterSetFileManager.Default;
                if (!fileManager.CharacterSetFileExists(filenameArg))
                {
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                        $"Character set file '{filenameArg}' not found.\n" +
                        $"Directory: {fileManager.GetCharacterSetDirectory()}"));
                }

                // Resolve clan
                string clanArg = parsed.GetArgument("clan", 1);
                if (string.IsNullOrWhiteSpace(clanArg))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Clan argument cannot be empty."));

                EntityFinderResult<Clan> clanResult = ClanFinder.FindSingleClan(clanArg);
                if (!clanResult.IsSuccess)
                    return CommandResult.Error(clanResult.Message);

                Clan targetClan = clanResult.Entity;

                // Parse type override
                string typeArg = parsed.GetArgument("type", 2);
                if (typeArg != null)
                {
                    string lowerType = typeArg.ToLower();
                    if (lowerType != "lord" && lowerType != "wanderer" && lowerType != "companion")
                    {
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(
                            $"Invalid hero type '{typeArg}'. Valid types: lord, wanderer, companion"));
                    }
                }

                // Parse settlement
                Settlement settlement = null;
                string settlementArg = parsed.GetArgument("settlement", 3);
                if (settlementArg != null && settlementArg.ToLower() != "null")
                {
                    EntityFinderResult<Settlement> settlementResult = SettlementFinder.FindSingleSettlement(settlementArg);
                    if (!settlementResult.IsSuccess)
                        return CommandResult.Error(settlementResult.Message);

                    settlement = settlementResult.Entity;
                }

                // Parse withParty
                bool withParty = true;
                string withPartyArg = parsed.GetArgument("withParty", 4) ?? parsed.GetNamed("party");
                if (withPartyArg != null)
                {
                    if (!CommandValidator.ValidateBoolean(withPartyArg, out withParty, out string boolError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(boolError));
                }

                if (!CommandValidator.ValidateHeroCreationLimit(1, out string limitError))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(limitError));

                // MARK: Execute Logic
                Dictionary<string, string> resolvedValues = new()
                {
                    { "filename", filenameArg },
                    { "clan", targetClan.Name.ToString() },
                    { "type", typeArg ?? "From file" },
                    { "settlement", settlement != null ? settlement.Name.ToString() : "Auto-resolved" },
                    { "withParty", withParty.ToString() }
                };

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.import_character", resolvedValues);

                string filepath = fileManager.GetCharacterSetFilePath(filenameArg);
                BLGMResult importResult = fileManager.ImportCharacterSet(filepath, targetClan, typeArg, settlement, withParty);

                if (!importResult.IsSuccess)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(importResult.Message));

                return CommandResult.Success(argumentDisplay + MessageFormatter.FormatSuccessMessage(importResult.Message));
            }).Message;
        }
    }
}
