using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;

namespace Bannerlord.GameMaster.BLGMDebug
{
    public static class HeroDebug
    {
        /// <summary>
        /// Check captain perks and compatible perks <br />
        /// Automaticall writes to log and returns the result as well
        /// </summary>
        public static string CaptainOnFootPerks(Hero hero)
        {
            string debugInfo = "";

            debugInfo += $"=============================={hero.Name}==============================";
            debugInfo += "\n---------GetCaptainPerksForTroopUsages---------";
            debugInfo += "\nCaptain Perks:";
            IEnumerable<PerkObject> perks = Helpers.PerkHelper.GetCaptainPerksForTroopUsages(TroopUsageFlags.OnFoot);
            foreach (PerkObject perk in perks)
            {
                debugInfo+= $"\nOnFootCaptainPerk:{perk.Name}, skill:{perk.Skill}";
            }

            debugInfo += "\n---------GetCaptainRatingForTroopUsages---------";
            debugInfo += "\nComaptible Perks:";
            DefaultBattleCaptainModel model = new(); model.GetCaptainRatingForTroopUsages(hero, TroopUsageFlags.OnFoot, out List<PerkObject> perks2);
            foreach (PerkObject perk in perks2)
            {
                debugInfo += $"\nOnFootCaptainPerk:{perk.Name}, skill:{perk.Skill}";
            }

            TaleWorlds.Library.Debug.Print(debugInfo);
            return debugInfo;
        }
    }
}