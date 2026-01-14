using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.RemovalHelpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console
{
    [CommandLineFunctionality.CommandLineArgumentFunction("cleanup", "gm")]
    public static class CleanupCommands
    {
        // MARK: Remove Single Hero
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_blgm_hero", "gm.cleanup")]
        public static string RemoveBlgmHero(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error)) return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.cleanup.remove_blgm_hero <hero>",
                    "Removes a single BLGM-generated hero\n- hero: Hero identifier (name or ID)",
                    "gm.cleanup.remove_blgm_hero blgm_hero_123"
                );

                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                if (parsedArgs.TotalCount < 1) return usageMessage;

                string heroIdentifier = parsedArgs.GetPositional(0);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", heroIdentifier }
                };

                string argumentDisplay = parsedArgs.FormatArgumentDisplay("remove_blgm_hero", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    (Hero hero, string heroError) = CommandBase.FindSingleHero(heroIdentifier);
                    if (heroError != null) return heroError;

                    BLGMResult result = HeroRemover.RemoveSingleHero(hero);

                    if (result.wasSuccessful)
                    {
                        return argumentDisplay + CommandBase.FormatSuccessMessage(result.message);
                    }
                    else
                    {
                        return argumentDisplay + result.message;
                    }
                }, "remove_blgm_hero");
            });
        }

        // MARK: Batch Remove Heroes
        [CommandLineFunctionality.CommandLineArgumentFunction("batch_remove_heroes", "gm.cleanup")]
        public static string BatchRemoveHeroes(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error)) return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.cleanup.batch_remove_heroes [count]",
                    "Removes multiple BLGM-generated heroes\n- count: Number to remove (optional, removes all if not specified)",
                    "gm.cleanup.batch_remove_heroes 5"
                );

                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                int? count = null;
                object resolvedCount = "All";

                if (parsedArgs.TotalCount >= 1)
                {
                    string countStr = parsedArgs.GetPositional(0);
                    if (!CommandValidator.ValidateIntegerRange(countStr, 1, int.MaxValue, out int countValue, out string countError))
                    {
                        return countError;
                    }
                    
                    count = countValue;
                    resolvedCount = count.Value;
                }

                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", resolvedCount.ToString() }
                };

                string argumentDisplay = parsedArgs.FormatArgumentDisplay("batch_remove_heroes", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    (int removed, string details) = HeroRemover.BatchRemoveHeroes(count);

                    return argumentDisplay + CommandBase.FormatSuccessMessage($"Removed {removed} BLGM hero(es)\n{details}");
                }, "batch_remove_heroes");
            });
        }

        // MARK: Batch Remove Parties
        [CommandLineFunctionality.CommandLineArgumentFunction("batch_remove_blgm_parties", "gm.cleanup")]
        public static string BatchRemoveBlgmParties(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error)) return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.cleanup.batch_remove_blgm_parties [count]",
                    "Removes mobile parties led by BLGM-generated heroes\n- count: Number to remove (optional, removes all if not specified)",
                    "gm.cleanup.batch_remove_blgm_parties 10"
                );

                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                int? count = null;
                object resolvedCount = "All";

                if (parsedArgs.TotalCount >= 1)
                {
                    string countStr = parsedArgs.GetPositional(0);
                    if (!CommandValidator.ValidateIntegerRange(countStr, 1, int.MaxValue, out int countValue, out string countError))
                    {
                        return countError;
                    }
                    
                    count = countValue;
                    resolvedCount = count.Value;
                }

                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", resolvedCount.ToString() }
                };

                string argumentDisplay = parsedArgs.FormatArgumentDisplay("batch_remove_blgm_parties", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    (int removed, string details) = PartyRemover.BatchRemoveParties(count);

                    return argumentDisplay + CommandBase.FormatSuccessMessage($"Removed {removed} BLGM party(ies)\n{details}");
                }, "batch_remove_blgm_parties");
            });
        }

        // MARK: Remove Single Clan
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_blgm_clan", "gm.cleanup")]
        public static string RemoveBlgmClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error)) return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.cleanup.remove_blgm_clan <clan>",
                    "Removes a single BLGM-generated clan\n- clan: Clan identifier (name or ID)",
                    "gm.cleanup.remove_blgm_clan blgm_clan_123"
                );

                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                if (parsedArgs.TotalCount < 1) return usageMessage;

                string clanIdentifier = parsedArgs.GetPositional(0);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "clan", clanIdentifier }
                };

                string argumentDisplay = parsedArgs.FormatArgumentDisplay("remove_blgm_clan", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    (Clan clan, string clanError) = CommandBase.FindSingleClan(clanIdentifier);
                    if (clanError != null) return clanError;

                    BLGMResult result = ClanRemover.RemoveSingleClan(clan);

                    if (result.wasSuccessful)
                    {
                        return argumentDisplay + CommandBase.FormatSuccessMessage(result.message);
                    }
                    else
                    {
                        return argumentDisplay + result.message;
                    }
                }, "remove_blgm_clan");
            });
        }

        // MARK: Batch Remove Clans
        [CommandLineFunctionality.CommandLineArgumentFunction("batch_remove_blgm_clans", "gm.cleanup")]
        public static string BatchRemoveBlgmClans(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error)) return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.cleanup.batch_remove_blgm_clans [count]",
                    "Removes multiple BLGM-generated clans\n- count: Number to remove (optional, removes all if not specified)",
                    "gm.cleanup.batch_remove_blgm_clans 3"
                );

                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                int? count = null;
                object resolvedCount = "All";

                if (parsedArgs.TotalCount >= 1)
                {
                    string countStr = parsedArgs.GetPositional(0);
                    if (!CommandValidator.ValidateIntegerRange(countStr, 1, int.MaxValue, out int countValue, out string countError))
                    {
                        return countError;
                    }
                    
                    count = countValue;
                    resolvedCount = count.Value;
                }

                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", resolvedCount.ToString() }
                };

                string argumentDisplay = parsedArgs.FormatArgumentDisplay("batch_remove_blgm_clans", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    (int removed, string details) = ClanRemover.BatchRemoveClans(count);

                    return argumentDisplay + CommandBase.FormatSuccessMessage($"Removed {removed} BLGM clan(s)\n{details}");
                }, "batch_remove_blgm_clans");
            });
        }

        // MARK: Remove Single Kingdom
        [CommandLineFunctionality.CommandLineArgumentFunction("remove_blgm_kingdom", "gm.cleanup")]
        public static string RemoveBlgmKingdom(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error)) return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.cleanup.remove_blgm_kingdom <kingdom>",
                    "Removes a single BLGM-generated kingdom\n- kingdom: Kingdom identifier (name or ID)",
                    "gm.cleanup.remove_blgm_kingdom blgm_kingdom_123"
                );

                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                if (parsedArgs.TotalCount < 1) return usageMessage;

                string kingdomIdentifier = parsedArgs.GetPositional(0);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "kingdom", kingdomIdentifier }
                };

                string argumentDisplay = parsedArgs.FormatArgumentDisplay("remove_blgm_kingdom", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    (Kingdom kingdom, string kingdomError) = CommandBase.FindSingleKingdom(kingdomIdentifier);
                    if (kingdomError != null) return kingdomError;

                    BLGMResult result = KingdomRemover.RemoveSingleKingdom(kingdom);

                    if (result.wasSuccessful)
                    {
                        return argumentDisplay + CommandBase.FormatSuccessMessage(result.message);
                    }
                    else
                    {
                        return argumentDisplay + result.message;
                    }
                }, "remove_blgm_kingdom");
            });
        }

        // MARK: Batch Remove Kingdoms
        [CommandLineFunctionality.CommandLineArgumentFunction("batch_remove_blgm_kingdoms", "gm.cleanup")]
        public static string BatchRemoveBlgmKingdoms(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignState(out string error)) return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.cleanup.batch_remove_blgm_kingdoms [count]",
                    "Removes multiple BLGM-generated kingdoms\n- count: Number to remove (optional, removes all if not specified)",
                    "gm.cleanup.batch_remove_blgm_kingdoms 2"
                );

                CommandBase.ParsedArguments parsedArgs = CommandBase.ParseArguments(args);

                int? count = null;
                object resolvedCount = "All";

                if (parsedArgs.TotalCount >= 1)
                {
                    string countStr = parsedArgs.GetPositional(0);
                    if (!CommandValidator.ValidateIntegerRange(countStr, 1, int.MaxValue, out int countValue, out string countError))
                    {
                        return countError;
                    }
                    
                    count = countValue;
                    resolvedCount = count.Value;
                }

                Dictionary<string, string> resolvedValues = new()
                {
                    { "count", resolvedCount.ToString() }
                };

                string argumentDisplay = parsedArgs.FormatArgumentDisplay("batch_remove_blgm_kingdoms", resolvedValues);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    (int removed, string details) = KingdomRemover.BatchRemoveKingdoms(count);

                    return argumentDisplay + CommandBase.FormatSuccessMessage($"Removed {removed} BLGM kingdom(s)\n{details}");
                }, "batch_remove_blgm_kingdoms");
            });
        }
    }
}
