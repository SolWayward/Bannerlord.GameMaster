using Bannerlord.GameMaster.Common;
using Bannerlord.GameMaster.Console.Common;
using Bannerlord.GameMaster.Console.Common.EntityFinding;
using Bannerlord.GameMaster.Console.Common.Execution;
using Bannerlord.GameMaster.Console.Common.Formatting;
using Bannerlord.GameMaster.Console.Common.Parsing;
using Bannerlord.GameMaster.Console.Common.Validation;
using Bannerlord.GameMaster.Items;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Bannerlord.GameMaster.Console.HeroCommands.HeroManagementCommands
{
    /// <summary>
    /// Console command to equip a hero with stat-based equipment using the new HeroOutfitter system.
    /// Analyzes hero skills to determine appropriate weapon loadout and equipment tier.
    /// Usage: gm.hero.equip_hero &lt;hero&gt; [tier] [civilian]
    /// </summary>
    public static class EquipHeroCommand
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("equip_hero", "gm.hero")]
        public static string EquipHero(List<string> args)
        {
            return Cmd.Run(args, () =>
            {
                // MARK: Validation
                if (!CommandValidator.ValidateCampaignState(out string error))
                    return CommandResult.Error(error);

                string usageMessage = CommandValidator.CreateUsageMessage(
                    "gm.hero.equip_hero", "<hero> [tier] [civilian]",
                    "Equips a hero with level-appropriate equipment based on their combat skills.\n" +
                    "The system analyzes the hero's skills to determine optimal weapon loadout.\n" +
                    "- hero: required, hero name or ID. Use 'player' for the main character\n" +
                    "- tier: optional, equipment tier (0+). Native items are tier 0-6, mods may add higher. Defaults to tier apropiate for hero level\n" +
                    "- civilian: optional, also replace civilian equipment (true/false). Defaults to false\n" +
                    "Supports named arguments: hero:player tier:5 civilian:true",
                    "gm.hero.equip_hero player\n" +
                    "gm.hero.equip_hero hero:player tier:5\n" +
                    "gm.hero.equip_hero 'Lucon of the Empire' tier:4\n" +
                    "gm.hero.equip_hero hero:'Garios of the Empire' tier:6 civilian:true");

                ParsedArguments parsed = ArgumentParser.ParseArguments(args);

                parsed.SetValidArguments(
                    new ArgumentDefinition("hero", true, null, "h"),
                    new ArgumentDefinition("tier", false, "auto", "t"),
                    new ArgumentDefinition("civilian", false, "false", "civ")
                );

                string validationError = parsed.GetValidationError();
                if (validationError != null)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage(validationError));

                if (parsed.TotalCount < 1)
                    return CommandResult.Success(usageMessage);

                // MARK: Parse Arguments
                string heroArg = parsed.GetArgument("hero", 0) ?? parsed.GetNamed("h");
                if (string.IsNullOrWhiteSpace(heroArg))
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage("Hero argument cannot be empty."));

                EntityFinderResult<Hero> heroResult = HeroFinder.FindSingleHero(heroArg);
                if (!heroResult.IsSuccess)
                    return CommandResult.Error(heroResult.Message);
                Hero hero = heroResult.Entity;

                if (!hero.IsAlive)
                    return CommandResult.Error(MessageFormatter.FormatErrorMessage($"Cannot equip {hero.Name} - hero is dead."));

                // Parse tier
                int tier = -1; // -1 means auto-calculate from hero level
                string tierArg = parsed.GetNamed("tier") ?? parsed.GetNamed("t");
                if (tierArg == null && parsed.PositionalCount > 1)
                {
                    // Check if second positional is a number (tier) or boolean (civilian)
                    string secondArg = parsed.GetPositional(1);
                    if (int.TryParse(secondArg, out int _))
                    {
                        tierArg = secondArg;
                    }
                }

                if (tierArg != null)
                {
                    if (!CommandValidator.ValidateIntegerRange(tierArg, 0, int.MaxValue, out tier, out string tierError))
                        return CommandResult.Error(MessageFormatter.FormatErrorMessage(tierError));
                }

                // Parse civilian
                bool civilian = false;
                string civilianArg = parsed.GetNamed("civilian") ?? parsed.GetNamed("civ");
                if (civilianArg == null)
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
                else
                {
                    civilian = civilianArg.ToLowerInvariant() == "true";
                }

                // MARK: Execute Logic
                int effectiveTier = tier >= 0 ? tier : HeroEquipper.CalculateTierFromLevel(hero.Level);
                WeaponTypeFlags derivedPreferences = HeroEquipper.DeriveWeaponPreferencesFromSkills(hero);

                Dictionary<string, string> resolvedValues = new()
                {
                    { "hero", hero.Name.ToString() },
                    { "tier", tier >= 0 ? tier.ToString() : $"auto ({effectiveTier})" },
                    { "civilian", civilian.ToString().ToLowerInvariant() }
                };

                HeroEquipper equipper = new();
                BLGMResult result = equipper.EquipHeroByStats(
                    hero,
                    tier,
                    WeaponTypeFlags.None, // Let the system derive from skills
                    replaceBattleEquipment: true,
                    replaceCivilianEquipment: civilian);

                string argumentDisplay = parsed.FormatArgumentDisplay("gm.hero.equip_hero", resolvedValues);

                if (!result.IsSuccess)
                    return CommandResult.Error(argumentDisplay + MessageFormatter.FormatErrorMessage(result.Message));

                string weaponInfo = GetWeaponPreferencesDescription(derivedPreferences);
                string civilianInfo = civilian ? " (including civilian equipment)" : "";

                string fullMessage = argumentDisplay + MessageFormatter.FormatSuccessMessage(
                    $"Equipped {hero.Name} with tier {effectiveTier} equipment{civilianInfo}.\n" +
                    $"Derived loadout: {weaponInfo}\n" +
                    $"Hero Level: {hero.Level}, Culture: {hero.Culture?.StringId ?? "none"}");

                return CommandResult.Success(fullMessage);
            }).Log().Message;
        }

        #region Helper Methods

        /// <summary>
        /// Gets a human-readable description of weapon type flags.
        /// </summary>
        /// <param name="flags">The weapon type flags to describe.</param>
        /// <returns>A string describing the weapon preferences.</returns>
        private static string GetWeaponPreferencesDescription(WeaponTypeFlags flags)
        {
            List<string> parts = new();

            // Check melee weapons
            if ((flags & WeaponTypeFlags.AllOneHanded) != WeaponTypeFlags.None)
                parts.Add("One-Handed");
            if ((flags & WeaponTypeFlags.AllTwoHanded) != WeaponTypeFlags.None)
                parts.Add("Two-Handed");
            if ((flags & WeaponTypeFlags.AllPolearms) != WeaponTypeFlags.None)
                parts.Add("Polearm");

            // Check ranged weapons
            if ((flags & WeaponTypeFlags.Bow) != WeaponTypeFlags.None)
                parts.Add("Bow");
            if ((flags & WeaponTypeFlags.Crossbow) != WeaponTypeFlags.None)
                parts.Add("Crossbow");
            if ((flags & WeaponTypeFlags.AllThrowing) != WeaponTypeFlags.None)
                parts.Add("Throwing");

            // Check shield
            if ((flags & WeaponTypeFlags.Shield) != WeaponTypeFlags.None)
                parts.Add("Shield");

            if (parts.Count == 0)
                return "Default (One-Handed + Shield)";

            return string.Join(", ", parts);
        }

        #endregion
    }
}
