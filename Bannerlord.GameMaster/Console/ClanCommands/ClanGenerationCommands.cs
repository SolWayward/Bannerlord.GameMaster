using Bannerlord.GameMaster.Console.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using System.Reflection;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using Bannerlord.GameMaster.Clans;

namespace Bannerlord.GameMaster.Console.ClanCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("clan", "gm")]
    public static class ClanGenerationCommands
    {
        /// <summary>
        /// Create a clan and set a hero to be it's leader. and optional set assign the clan to kingdom. Clan is independent if no kingdom
        /// Usage: gm.clan.add_hero <clan> <hero> [kingdom]
        /// </summary>
        [CommandLineFunctionality.CommandLineArgumentFunction("create_clan", "gm.clan")]
        public static string CreateClan(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                if (!CommandBase.ValidateCampaignMode(out string error))
                    return error;

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.clan.create_clan", "<clanName> <leaderHero> [kingdom]",
                    "Create a new clan with the specified name and set the specified hero as its leader.\n" +
                    "Optionally, specify a kingdom for clan, if no kingdom specified, clan is independent.\n" +
                    "Use SINGLE QUOTES for multi-word clan names (double quotes don't work).",
                    "gm.clan.create_clan Highlanders derthert\ngm.clan.create_clan 'The Highland Clan' derthert");

                if (!CommandBase.ValidateArgumentCount(args, 2, usageMessage, out error))
                    return error;

                var (hero, heroError) = CommandBase.FindSingleHero(args[1]);
                if (heroError != null) return heroError;

                string clanName = args[0];

                Clan newClan = ClanGenerator.CreateClan(clanName, hero);

                return CommandBase.ExecuteWithErrorHandling(() =>
                {
                    return CommandBase.FormatSuccessMessage($"Created clan named {clanName} lead by {hero.Name}");
                }, "Failed to create clan");
            }); 
        }
    }
}